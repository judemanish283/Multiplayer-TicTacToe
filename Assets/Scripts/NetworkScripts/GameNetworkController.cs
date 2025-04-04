using System;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkController : MonoBehaviour
{
    public static GameNetworkController Instance;

    public static event Action OnGameStarted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void HostGame() 
    {
        if (NetworkManager.Singleton.StartHost()) 
        {
            OnGameStarted?.Invoke();
        }
    }

    public void JoinGame() 
    {
        if (NetworkManager.Singleton.StartClient()) 
        {
            OnGameStarted?.Invoke();
        }
    }
}
