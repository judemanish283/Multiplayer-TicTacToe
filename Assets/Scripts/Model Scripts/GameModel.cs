using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum eType
{
    None, X, O
}

public struct WinLine
{
    public List<Vector2Int> gridPos;
    public float zRotation;
    public Vector2 lineRotation;
}
public class GameModel : NetworkBehaviour
{
    private eType[,] boardState = new eType[3, 3];

    [SerializeField] private eType localplayerType = eType.None;

    private int numberOfTilesFilled = 0;

    private List<WinLine> allWinLines;

    public bool isRoundWinner = false;

    private float lineZRotation;
    private Vector2 lineImagePos;

    private void Awake()
    {
        allWinLines = new List<WinLine>
        {
        // Columns
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) }
                    ,lineRotation = new Vector2(-300f,-45f), zRotation = 90f },
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) }
                    ,lineRotation = new Vector2(0f,-45f), zRotation = 90f },
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) }
                    ,lineRotation = new Vector2(300f,-45f), zRotation = 90f },

        // Rows
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }
                    ,lineRotation = new Vector2(0f,270f), zRotation = 0f },
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) }
                    ,lineRotation = new Vector2(0f,-35f), zRotation = 0f },
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) }
                    ,lineRotation = new Vector2(0f,-350f), zRotation = 0f },

        // Diagonals
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) }
                    ,lineRotation = new Vector2(0f,-35f), zRotation = -45f },
        new WinLine { gridPos = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) }
                    ,lineRotation = new Vector2(0f,-35f), zRotation = 45f },
        };

    }

    public bool CheckIfTileIsEmpty(Vector2Int tileCoords)
    {
        return boardState[tileCoords.x, tileCoords.y] == eType.None;
    }

    public void SetTileState(Vector2Int tileCoords, eType stateToSet)
    {
        boardState[tileCoords.x, tileCoords.y] = stateToSet;
    }

    public void SetPlayerType()
    {
        Debug.Log("Setting PlayerType");
        eType typeToSet = (NetworkManager.Singleton.LocalClientId == 0) ? eType.X : eType.O;
        localplayerType = typeToSet;
        Debug.Log("PLayer types is : " + typeToSet.ToString());
    }

    public eType GetPlayerType() => localplayerType;

    public void IncreaseTilesFilled()
    {
        numberOfTilesFilled++;
        Debug.Log("Number of Tiles Filled increased : " + numberOfTilesFilled);
    }

    public void CheckAllTiles()
    {
        foreach (var line in allWinLines)
        {
            foreach (var pos in line.gridPos)
            {
                Debug.Log($"tile at pos {pos.x},{pos.y} is of type {boardState[pos.x, pos.y]}");
            }
        }
    }

    public bool CheckForWinnerInAllLines() 
    {
        if (numberOfTilesFilled >= 5) 
        {
            for (int i = 0; i < allWinLines.Count; i++)
            {
                if (CheckForWinnerInThisLine(allWinLines[i]))
                {
                    return true;
                }
            }
        }        
        return false;
    }

    

    public bool CheckForWinnerInThisLine(WinLine lineToCheck) 
    {
        eType typeToCheck = boardState[lineToCheck.gridPos[0].x, lineToCheck.gridPos[0].y];
        for (int i = 0; i < 3; i++) 
        {
            Vector2Int gridCoord = lineToCheck.gridPos[i];
            if (boardState[gridCoord.x, gridCoord.y] != eType.None && boardState[gridCoord.x, gridCoord.y] == typeToCheck)
            {
                continue;
            }
            else return false;
        }
        if (typeToCheck == localplayerType) isRoundWinner = true;
        lineZRotation = lineToCheck.zRotation;
        lineImagePos = lineToCheck.lineRotation;
        return true;
    } 

    public (Vector2,float) ReturnPatternPositions() => (lineImagePos,lineZRotation);


    public int GetTilesFilled() => numberOfTilesFilled;

    public void ResetAllTiles() 
    {
        for (int i = 0; i < boardState.GetLength(0); i++)
            for (int j = 0; j < boardState.GetLength(1); j++)
                boardState[i, j] = eType.None;
        isRoundWinner = false;
        numberOfTilesFilled = 0;
    }
}
