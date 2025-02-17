using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using Unity.Netcode;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode.Transports;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;

public class MyNetwork : MonoBehaviour
{
    public Toggle hostIsPlayerToggle;

    public static bool host_isPlayer;

    public static string player_name;
    public static Color player_color;
    public TMP_InputField player_nameInput;
    public FlexibleColorPicker player_colorInput;
    public TMP_InputField player_joinCode;

    //public TMP_InputField host_nameInput;
    //public TMP_InputField host_PasswordInput;

    public Animator skeletonAnimator;
    public AudioSource thunderAudioSource;

    public Button hostButton;
    public Button playerButton;

    //public static Dictionary<ulong, string> IdToName = new Dictionary<ulong, string>();
    public static HashSet<string> allPlayerNames = new HashSet<string>();

    bool doOnce = false;
    public RelayHostData relayHostData;
    public RelayJoinData relayJoinData;
    private string lobbyId;
    public string LobbyCode;

    async void Awake()
    {
        if (doOnce) return;

        doOnce = true;

        try
        {

            UnityTransport transport = GetComponent<UnityTransport>();

            transport.SetConnectionData(GetLocalIPv4(), (ushort)8888);

            
            //host_nameInput.text = RandomString(10);
            await Authenticate(RandomString(20));
            await SignIn();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            WrongInformation(e.Message);
        }
    }

    private void Update()
    {
        if(Application.isFocused)
            Application.targetFrameRate = 60;
        else
            Application.targetFrameRate = 5;
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList.First(
        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }

    public static string RandomString(int length)
    {
        System.Random random = new System.Random(DateTime.Now.GetHashCode());
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #region Auth Events



    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    public async Task Authenticate(string playerName)
    {
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);
    }

    async Task SignIn()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    #endregion 

    public async void StartClientButton()
    {
        playerButton.interactable = false;
        hostButton.interactable = false;
        await StartClient();
    }
    public async void StartHostButton()
    {
        playerButton.interactable = false;
        hostButton.interactable = false;
        await StartHost();
    }
    public async Awaitable StartClient()
    {
        if (!Player_CheckInfo())
        {
            return;
        }

        if(player_joinCode.text == "")
        {
            WrongInformation("ERROR: no lobby code");
            return;
        }

        JoinAllocation allocation = null;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(player_joinCode.text);
            LobbyCode = player_joinCode.text.ToUpper();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ChatSystem.SystemSendMessage(e.Message);
        }

        relayJoinData = new RelayJoinData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };

        UnityTransport unityTransport = GetComponent<UnityTransport>();

        unityTransport.SetRelayServerData(relayJoinData.IPv4Address,
            relayJoinData.Port,
            relayJoinData.AllocationIDBytes,
            relayJoinData.Key,
            relayJoinData.ConnectionData,
            relayJoinData.HostConnectionData);

        
        NetworkManager.Singleton.StartClient();
    }
    public async Awaitable StartHost()
    {
        
        host_isPlayer = hostIsPlayerToggle.isOn;

        if(!Host_CheckInfo())
        {
            return;
        }


        Allocation allocation = null;

        try
        {
            //Ask Unity Services to allocate a Relay server
            allocation = await RelayService.Instance.CreateAllocationAsync(5);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ChatSystem.SystemSendMessage(e.Message);
        }

        //Populate the hosting data
        relayHostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };

        try
        {
            //Retrieve the Relay join code for our clients to join our party
            relayHostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(relayHostData.AllocationID);
            LobbyCode = relayHostData.JoinCode;
            Debug.Log(relayHostData.JoinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            ChatSystem.SystemSendMessage(e.Message);
        }

        //Retrieve the Unity transport used by the NetworkManager
        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

        transport.SetRelayServerData(relayHostData.IPv4Address,
            relayHostData.Port,
            relayHostData.AllocationIDBytes,
            relayHostData.Key,
            relayHostData.ConnectionData);

        /*
        {
            var createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = false;
            createLobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: relayHostData.JoinCode
                    )
                }
            };
            lobby = await CreateLobbyWithHeartbeatAsync(createLobbyOptions);
            lobbyId = lobby.Id;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            ChatSystem.SystemSendMessage(e.Message);
        }*/

        await SceneManager.LoadSceneAsync("HeadScene");

        NetworkManager.Singleton.StartHost();

        if (host_isPlayer)
        {
            allPlayerNames.Add(player_name);
        }
    }

    

    public bool Player_CheckInfo()
    {
        if (player_nameInput.text != "") player_name = player_nameInput.text;
        else
        {
            WrongInformation("ERROR: no player name");
            return false;
        }

        if (player_colorInput.color != Color.white && player_colorInput.color != Color.black) player_color = player_colorInput.color;
        else
        {
            WrongInformation("ERROR: color cannot be black or white. be creative, dummy >:[");
            return false;
        }

        return true;
    }
    public bool Host_CheckInfo()
    {
        if(host_isPlayer && !Player_CheckInfo())
        {
            WrongInformation("ERROR: incorrect player credentials");
            return false;
        }

        /*if (host_nameInput.text == "") 
        {
            WrongInformation("ERROR: No lobby name");
            return false;
        }*/

        return true;
    }

    public void WrongInformation(string message)
    {
        thunderAudioSource.Play();
        skeletonAnimator.SetTrigger("WrongInfo");

        playerButton.interactable = true;
        hostButton.interactable = true;

        ChatSystem.SystemSendMessage(message, 10);
    }



    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }
}
