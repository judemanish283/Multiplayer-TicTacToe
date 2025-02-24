using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private List<Button> tileBtns;

    private void Start()
    {
        foreach(var tileBtn in tileBtns) 
        {
            tileBtn.onClick.AddListener(() => 
            {
                GameManager.Instance.TileClickedOnRpc(tileBtn.GetComponent<Tile>().GetTilePos());
                //UpdateTileRpc(tileBtns.FindIndex(tileBtn));
            });
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateTileRpc(int index)
    {
        if (!IsServer) return;
        ChangeTileRpc(index);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeTileRpc(int index)
    {
        tileBtns[index].GetComponent<Tile>().UpdateImage();
    }

     
    
}
