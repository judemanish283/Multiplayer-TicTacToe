using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameRelayManager : MonoBehaviour
{
    [SerializeField] Button hostBtn, joinBtn;
    [SerializeField] TMP_Text lobbyCodeTxt;
    [SerializeField] TMP_InputField joinLobbyInputField;

    public string relayJoinCode;

    void Start()
    {
        //await UnityServices.InitializeAsync();

        //await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //hostBtn.onClick.AddListener(() => CreateRelay());
        //joinBtn.onClick.AddListener(() => JoinRelay(relayJoinCode));

    }

    public async Task<string> CreateRelay() 
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
        relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        lobbyCodeTxt.text = $"Code : {relayJoinCode}";

        //GameController.Instance.lobbyCode = joinCode;

        var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

;
        //UIManager.Instance.ShowGameScreen();

        return NetworkManager.Singleton.StartHost() ? relayJoinCode : null;
    }

    public async Task<bool> JoinRelay(string joinCode) 
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

}
