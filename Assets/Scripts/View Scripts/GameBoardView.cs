using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class GameBoardView : MonoBehaviour
{     
    public const float MESSAGE_SHOW_DURATION = 1f;

    [SerializeField] List <Tile> tileButtons;

    [SerializeField] private Sprite xSprite, oSprite;

    [SerializeField] TMP_Text playerSymbolTxt, turnText, player1ScoreTxt, player2ScoreTxt,lobbyCodeText, yourNameTxt, OppNameTxt ;

    [SerializeField] GameObject messagePanel;
    [SerializeField] TMP_Text messageTxt;

    [SerializeField] Image lineImage;

    private void Start()
    {
        foreach(var tile in tileButtons)
        {
            tile.button.onClick.AddListener(() =>
            {
                Debug.Log(tile.GetTileCoord().ToString());
                Vector2Int tileCoord = tile.GetTileCoord();
                GameController.Instance.TileClickedOnRpc(tile.GetTileCoord(), NetworkManager.Singleton.LocalClientId);
            });
        }
    }

    public void SetPlayerNames(string myName, string oppName) 
    {
        yourNameTxt.text = $"{myName}'s\nScore";
        OppNameTxt.text = $"{oppName}'s\nScore";
    }

    public void SetSymbolText(eType playerType) 
    {
        playerSymbolTxt.text = (playerType == eType.X) ? "X" : "O";
        lobbyCodeText.text = GameController.Instance.lobbyCode;
    }

    public void SetSprite(Vector2Int tileCoord, eType state) 
    {
        Tile tile = tileButtons.Find(t => t.GetTileCoord() == tileCoord);
        if (tile != null) 
        {
            Sprite spriteToSet = (state == eType.X) ? xSprite : oSprite;
            tile.SetImage(spriteToSet);
        }
    }

    public void UpdateTurnText(int turnVal) 
    {
        Debug.Log("Dictionary Count is : " + GameController.Instance.PlayerNames.Count);
        turnText.text = (turnVal == 0) ? "X's Turn" : "O's Turn";
    }

    public IEnumerator ShowMessage(string Message) 
    {
        messagePanel.SetActive(true);
        messageTxt.text = Message;
        yield return new WaitForSeconds(MESSAGE_SHOW_DURATION);
        messagePanel.SetActive(false);
    }

    public void UpdateScoreText(int yourScore, int oppScore) 
    {
        player1ScoreTxt.text = yourScore.ToString();
        player2ScoreTxt.text = oppScore.ToString();
    }

    public void ResetAllTiles() 
    {
        foreach (var tile in tileButtons) tile.SetImage(null);
        lineImage.gameObject.SetActive(false);
    }

    //public Tile GetTile(Vector2Int tilePos) 
    //{
    //    return tileButtons.Find(x => x.GetTileCoord() == tilePos);
    //}

    public void SetImagePositions((Vector2, float) imagePos, bool isWinner)
    {
        Debug.Log($"Positions are {imagePos.Item1.x}, {imagePos.Item1.y}");
        lineImage.transform.localPosition = new Vector3(imagePos.Item1.x, imagePos.Item1.y, 0f);
        lineImage.transform.eulerAngles =new Vector3(0f,0f, imagePos.Item2);

        lineImage.color = isWinner ? Color.green : Color.red;
        lineImage.gameObject.SetActive(true);
    }
}
