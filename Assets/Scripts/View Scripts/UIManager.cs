using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Relay Lobby Objects")]
    [SerializeField] private GameObject directLobbyObjects;
    [SerializeField] Button hostBtn, joinBtn;

    [Header("Game Objects")]
    [SerializeField] private GameObject gameScreenObjects;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        //directLobbyObjects.SetActive(true);
        //gameScreenObjects.SetActive(false);
    }

    private void OnEnable()
    {
        //GameNetworkController.OnGameStarted += ShowGameScreen;
    }

    private void OnDisable()
    {
        //GameNetworkController.OnGameStarted -= ShowGameScreen;
    }
    void Start()
    {
        //hostBtn.onClick.AddListener(() => 
        //{
        //    GameNetworkController.Instance.HostGame();
        //});
        //joinBtn.onClick.AddListener(() =>
        //{
        //    GameNetworkController.Instance.JoinGame();
        //});
    }

    public void ShowGameScreen() 
    {
        directLobbyObjects.SetActive(false);
        gameScreenObjects.SetActive(true);
    } 

}
