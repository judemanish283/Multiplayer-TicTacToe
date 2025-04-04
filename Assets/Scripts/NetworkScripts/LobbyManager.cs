using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    private const int MAX_PLAYERS_COUNT = 2;

    [SerializeField] GameRelayManager relayManager;

    [SerializeField] Button closeBtn;

    [SerializeField] GameObject LobbyMasterObject;

    [Header("Create Profile Menu")]
    [SerializeField] GameObject createProfileMenuObj;
    [SerializeField] TMP_InputField profileNameInputField;
    [SerializeField] Button createProfileBtn;

    [Header("Main Menu")]
    [SerializeField] GameObject mainMenuObj;
    [SerializeField] Button createNewLobbyBtn;
    [SerializeField] Button joinLobbyMenuBtn;

    [Header("Create Lobby Menu")]
    [SerializeField] GameObject createLobbyMenuObj;
    [SerializeField] TMP_InputField enterLobbyNameInputfield;
    [SerializeField] Button createLobbyBtn;

    [Header("Join Lobby Menu")]
    [SerializeField] GameObject joinlobbyMenuObj;
    [SerializeField] Transform lobbiesContentParent;
    [SerializeField] Transform lobbiePrefab;

    [Header("Joined Lobby Menu")]
    [SerializeField] GameObject joinedLobbyMenuObj;
    [SerializeField] TMP_Text joinedLobbyNameTxt;
    [SerializeField] Transform joinedLobbyContentParent;
    [SerializeField] Transform lobbyMemberPrefab;
    [SerializeField] Button startMatchBtn;

    private string playerName;
    private Player playerData;
    public string joinedLobbyId;

    private async void Start() 
    {
        if (Instance == null) Instance = this;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        createProfileBtn.onClick.AddListener(() => CreateProfile());
        createNewLobbyBtn.onClick.AddListener(() => ShowCreateLobbyMenu());
        joinLobbyMenuBtn.onClick.AddListener(() => ShowJoinLobbyMenu());
        createLobbyBtn.onClick.AddListener(() => CreateLobby());

        startMatchBtn.onClick.AddListener(() => LobbyStart());
        
        ShowProfileScreen();
    }

    #region MenuScreens
    private void ShowProfileScreen()
    {
        createProfileMenuObj.SetActive(true);
        mainMenuObj.SetActive(false);
        createLobbyMenuObj.SetActive(false);
        joinlobbyMenuObj.SetActive(false);
    }
    private void ShowMainMenu() 
    {
        createProfileMenuObj.SetActive(false);
        mainMenuObj.SetActive(true);
    }
    private void ShowCreateLobbyMenu() 
    {
        mainMenuObj.SetActive(false);
        createLobbyMenuObj.SetActive(true);
        
    }
    private void ShowJoinLobbyMenu() 
    {
        mainMenuObj.SetActive(false);
        createLobbyMenuObj.SetActive(false);
        joinlobbyMenuObj.SetActive(true);
        ShowLobbies();
    }

    private void ShowJoinedLobbyInfoMenu() 
    {
        createLobbyMenuObj.SetActive(false);
        joinedLobbyMenuObj.SetActive(true);
    }

    public void HideLobby() 
    {
        UIManager.Instance.ShowGameScreen();
        LobbyMasterObject.SetActive(false);
    }

    #endregion

    public void CreateProfile() 
    {
        if (profileNameInputField.text == string.Empty) return;

        playerName = profileNameInputField.text;

        ShowMainMenu();

        PlayerDataObject playerDataObjectName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName);

        playerData = new Player(id : AuthenticationService.Instance.PlayerId, data :
                     new Dictionary<string, PlayerDataObject> {{ "Name", playerDataObjectName }});
    }

    public async void LobbyStart() 
    {
        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyId);
        string joinCode = await relayManager.CreateRelay();

        await LobbyService.Instance.UpdateLobbyAsync(joinedLobbyId, new UpdateLobbyOptions 
        {
            Data = new Dictionary<string, DataObject> { {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public,joinCode) } }
        });

        HideLobby();
    }

    public async void CreateLobby() 
    {
        if (enterLobbyNameInputfield.text == string.Empty) 
        {
            Debug.Log("Lobby name cannot be empty");
            return;
        }
        
        Lobby createdLobby = null;
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
        createLobbyOptions.IsPrivate = false;
        createLobbyOptions.Player = playerData;

        DataObject dataObjectJoinCode = new DataObject(DataObject.VisibilityOptions.Public, string.Empty);
        createLobbyOptions.Data = new Dictionary<string, DataObject> { { "JoinCode", dataObjectJoinCode } };

        try
        {
            createdLobby = await LobbyService.Instance.CreateLobbyAsync(enterLobbyNameInputfield.text, MAX_PLAYERS_COUNT, createLobbyOptions);
            joinedLobbyId = createdLobby.Id;

            UpdateJoinedLobbyInfo();
        }
        catch (LobbyServiceException e) 
        {
            Debug.Log(e);
        }

        LobbyHeartbeat(createdLobby);

        ShowJoinedLobbyInfoMenu();
    }

    private async void LobbyHeartbeat(Lobby lobby) 
    {
        while (true) 
        {
            if (lobby == null) return;

            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);

            await Task.Delay(15 * 1000);
        }
    }

    private async void ShowLobbies() 
    {
        while (Application.isPlaying && joinlobbyMenuObj.activeInHierarchy) 
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            foreach (Transform t in lobbiesContentParent) Destroy(t.gameObject);

            foreach (Lobby lobby in queryResponse.Results) 
            {
                Transform newLobbyItem = Instantiate(lobbiePrefab, lobbiesContentParent);
                JoinLobbyButton joinLobbyButton = newLobbyItem.GetComponent<JoinLobbyButton>();
                joinLobbyButton.lobbyId = lobby.Id;
                joinLobbyButton.needPassword = lobby.HasPassword;
                newLobbyItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = lobby.Name;
                newLobbyItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
            }

            await Task.Delay(1000);
        }
    }

    public async void JoinLobby(string lobbyId, bool needPassword) 
    {
        if (needPassword)
        {
            try
            {

            }
            catch (LobbyServiceException e) 
            {
                Debug.Log(e);
            }
        }
        else 
        {
            try 
            {
                await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, new JoinLobbyByIdOptions { Player = playerData });

                joinedLobbyId = lobbyId;

                ShowJoinedLobbyInfoMenu();

                UpdateJoinedLobbyInfo();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private bool isJoined = false;
    private async void UpdateJoinedLobbyInfo() 
    {
        while (Application.isPlaying && !isJoined ) 
        {
            if (string.IsNullOrEmpty(joinedLobbyId)) return;

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyId);

            if (!isJoined && lobby.Data["JoinCode"].Value != string.Empty)
            {
                await relayManager.JoinRelay(lobby.Data["JoinCode"].Value);
                isJoined = true;
                //joinedLobbyParent.SetActive(false);

                HideLobby();
                return;
            }

            if (AuthenticationService.Instance.PlayerId == lobby.HostId)
            {
                startMatchBtn.gameObject.SetActive(true);
            }
            else
            {
                startMatchBtn.gameObject.SetActive(false);
            }


            joinedLobbyNameTxt.text = lobby.Name;

            foreach (Transform t in joinedLobbyContentParent) Destroy(t.gameObject);

            foreach (var player in lobby.Players) 
            {
                string playerId = player.Id;
                string playerName = player.Data["Name"].Value;

                GameController.Instance.PlayerNames[playerId] = playerName;

                Transform lobbyMemberInfo = Instantiate(lobbyMemberPrefab, joinedLobbyContentParent);
                lobbyMemberInfo.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.Data["Name"].Value;
                lobbyMemberInfo.GetChild(1).GetComponent<TextMeshProUGUI>().text = (lobby.HostId == player.Id) ? "X" : "O";
                lobbyMemberInfo.GetChild(2).GetComponent<TextMeshProUGUI>().text = (lobby.HostId == player.Id)? "Host" : "Client";
            }
            await Task.Delay(1000);
        }
    }
}
