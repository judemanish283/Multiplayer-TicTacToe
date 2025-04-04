using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameController : NetworkBehaviour
{
    public static GameController Instance;

    private const string WAIT_FOR_PLAYER_MESSAGE = "Wait for another player to join";
    private const string WAIT_FOR_YOUR_TURN_MESSAGE = "Not your turn";
    private const string ROUND_WIN_MESSAGE = "Congrats! You Won!";
    private const string ROUND_LOSE_MESSAGE = "Better luck next time";
    private const string ROUND_DRAW_MESSAGE = "Draw ! Get ready for the next one.";

    public NetworkVariable<int> turnInteger = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> player1Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> player2Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private GameBoardView gameView;
    [SerializeField] private GameModel gameModel;

    public string lobbyCode;

    public Dictionary<string, string> PlayerNames = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn() 
    {
        SetPlayerTypeTextRpc();
        if (IsServer) 
        {
            turnInteger.Value = 0;
            player1Score.Value = 0;
            player2Score.Value = 0;
        }
        turnInteger.OnValueChanged += (oldval, newVal) => 
        {
            UpdateTurnTextRpc(newVal);
        };
        player1Score.OnValueChanged += (oldVal, newVal) => 
        {
            UpdateScoresRpc(newVal, player2Score.Value, true);
        };
        player2Score.OnValueChanged += (oldVal, newVal) =>
        {
            UpdateScoresRpc(newVal, player1Score.Value,false);
        };

        SendPlayerNames();
    }

    private void SendPlayerNames() 
    {
        var playerNamesList = PlayerNames.ToList();

        if(NetworkManager.Singleton.LocalClientId == 0)
        {
            gameView.SetPlayerNames(playerNamesList[0].Value, playerNamesList[1].Value);
        }
        else gameView.SetPlayerNames(playerNamesList[1].Value, playerNamesList[0].Value);
    }

    private void SwitchTurns() 
    {
        Debug.Log("Switching Turns " + turnInteger.Value);
        turnInteger.Value = (turnInteger.Value == 0) ? 1 : 0;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerTypeTextRpc() 
    {
        gameModel.SetPlayerType();
        gameView.SetSymbolText(gameModel.GetPlayerType());
        if (NetworkManager.Singleton.ConnectedClients.Count == 2) UpdateTurnTextRpc(turnInteger.Value);
    }

    [Rpc(SendTo.Server)]
    public void TileClickedOnRpc(Vector2Int tileCoord, ulong clientId) 
    {
        if(NetworkManager.Singleton.ConnectedClients.Count == 1) 
        {
            ShowMessageToSingleClientRpc(WAIT_FOR_PLAYER_MESSAGE,clientId);
            return;
        }
        Debug.Log($"TileClickedOn {tileCoord.x} , {tileCoord.y}");
        if (!CheckIfMyTurn(clientId)) 
        {
            ShowMessageToSingleClientRpc(WAIT_FOR_YOUR_TURN_MESSAGE, clientId);
            Debug.Log("Not your turn");
            return;
        }
        if (gameModel.CheckIfTileIsEmpty(tileCoord) && CheckIfMyTurn(clientId)) 
        {
            UpdateTileRpc(tileCoord, clientId);
            SwitchTurns();  
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateTurnTextRpc(int val) 
    {
        gameView.UpdateTurnText(val);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowMessageToSingleClientRpc(string message, ulong clientId) 
    {
        if (NetworkManager.Singleton.LocalClientId == clientId) 
        {
            StartCoroutine(gameView.ShowMessage(message));
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateTileRpc(Vector2Int tileCoord, ulong clientId) 
    {
        eType symbol = (clientId == 0) ? eType.X : eType.O;
        gameModel.SetTileState(tileCoord, symbol);
        gameView.SetSprite(tileCoord, symbol);
        gameModel.IncreaseTilesFilled();
        if (gameModel.CheckForWinnerInAllLines()) 
        {
            Debug.Log("WINNER FOUND");
            if(IsServer) RoundEndRpc();
            return;
        }
        if(gameModel.GetTilesFilled() == 9) 
        {
            Debug.Log("DrawGame");
            if (IsServer) RoundDrawRpc();
        }
    }
    [Rpc(SendTo.Server)]
    private void RoundDrawRpc()
    {
        ShowDrawMessageRpc();
        SwitchTurns();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowDrawMessageRpc()
    {
        StartCoroutine(gameView.ShowMessage(ROUND_DRAW_MESSAGE));
        StartCoroutine(ResetRound());
    }


        [Rpc(SendTo.Server)]
    private void RoundEndRpc() 
    {
        ShowWinAndLoseMessageRpc();
        if (gameModel.isRoundWinner)
        {
            Debug.Log("Increasing Player 1 Score");
            player1Score.Value += 1;
        }
        else
        {
            Debug.Log("Increasing Player 2 Score");
            player2Score.Value += 1;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowWinAndLoseMessageRpc() 
    {
        if (gameModel.isRoundWinner)
        {
            Debug.Log("CONGRATS you win this round");
            gameView.SetImagePositions(gameModel.ReturnPatternPositions(),true);
            StartCoroutine(gameView.ShowMessage(ROUND_WIN_MESSAGE));
        }
        else 
        {
            Debug.Log("Better luck next time");
            gameView.SetImagePositions(gameModel.ReturnPatternPositions(), false);
            StartCoroutine(gameView.ShowMessage(ROUND_LOSE_MESSAGE));
        }
    }

    public bool CheckIfMyTurn(ulong clientId) 
    {
        return clientId == (ulong)turnInteger.Value;
    }

    [Rpc(SendTo.Server)]
    private void UpdateScoresRpc(int winnerScore, int loserScore, bool player1) //Player who won Score and opponent score
    {
        if(player1) ScoreUpdateRpc(winnerScore,loserScore);
        else ScoreUpdateRpc(loserScore, winnerScore);
        turnInteger.Value = 0; //To set the turn to X

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ScoreUpdateRpc(int player1Score, int player2Score)
    {
        StartCoroutine(DelayedScoreUpdate(player1Score, player2Score));
    }

    private IEnumerator DelayedScoreUpdate(int player1Score, int player2Score)
    {
        if (NetworkManager.Singleton.LocalClientId == 0) // You are player 1
        {
            gameView.UpdateScoreText(player1Score, player2Score);
        }
        else
        {
            gameView.UpdateScoreText(player2Score, player1Score);
        }
        yield return null;
        StartCoroutine(ResetRound());
    }

    private IEnumerator ResetRound() 
    {
        yield return new WaitForSeconds(3f);
        gameModel.ResetAllTiles();
        gameView.ResetAllTiles();
    }
}
