using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Lobby Objects")]
    [SerializeField] private GameObject lobbyObjects;
    [SerializeField] Button hostBtn, joinBtn;

    [Header("Game Objects")]
    [SerializeField] private GameObject gameScreenObjects;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        lobbyObjects.SetActive(true);
        gameScreenObjects.SetActive(false);
    }
    void Start()
    {
        hostBtn.onClick.AddListener(() => 
        {
            NetworkManager.Singleton.StartHost();
            lobbyObjects.SetActive(false);
            gameScreenObjects.SetActive(true);
        });
        joinBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            lobbyObjects.SetActive(false);
            gameScreenObjects.SetActive(true);
        });
    }

}
