using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyButton : MonoBehaviour
{
    public bool needPassword;
    public string lobbyId;
    private Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinLobby(lobbyId, needPassword);
        });
    }

}
