using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public enum eTileState
{
    None,
    X,
    O
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public eTileState[,] TileStates;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        TileStates = new eTileState[3, 3];
    }

    [Rpc(SendTo.Server)]
    public void TileClickedOnRpc(Vector2 pos)
    {
        if(!IsServer) return;
        Debug.Log("Button Clicked on");
        UpdateGridInfo(pos);
    }

    private void UpdateGridInfo(Vector2 pos)
    {
        TileStates[(int)pos.x, (int)pos.y] = eTileState.X;
        Debug.Log($"Tile at position {(int)pos.x},{(int)pos.y} is eTileState.X");
    }
}
