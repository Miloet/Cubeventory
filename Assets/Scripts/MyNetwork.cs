using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;
using System.Linq;

public class MyNetwork : MonoBehaviour
{
    public Lobby lobby;

    public Toggle hostIsPlayerToggle;

    public static bool host_isPlayer;

    public static string player_name;
    public static Color player_color;
    public TMP_InputField player_nameInput;
    public FlexibleColorPicker player_colorInput;
    public TMP_InputField player_joinCode;

    public TMP_InputField host_nameInput;
    //public TMP_InputField host_PasswordInput;

    public Animator skeletonAnimator;
    public AudioSource thunderAudioSource;

    public Button hostButton;
    public Button playerButton;

    public static HashSet<string> allPlayerNames = new HashSet<string>();

    async void Awake()
    {
        try
        {
            host_nameInput.text = RandomString(10);
            await Authenticate(RandomString(20));
            await SignIn();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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

        try
        {
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(player_joinCode.text);
            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            WrongInformation("ERROR: failed to join lobby");
        }
    }
    public async Awaitable StartHost()
    {
        host_isPlayer = hostIsPlayerToggle.isOn;

        if(!Host_CheckInfo())
        {
            return;
        }

        try
        {
            lobby = await CreateLobbyWithHeartbeatAsync();
            await SceneManager.LoadSceneAsync("HeadScene");

            NetworkManager.Singleton.StartHost();

            if (host_isPlayer)
            {
                allPlayerNames.Add(player_name);
            }
            //NetworkManager.Singleton.SceneManager.OnLoadComplete += NetworkManagerStartHost;
            //NetworkManager.Singleton.SceneManager.LoadScene("HeadScene", LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            WrongInformation("ERROR: lobby failed to start :(");
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

        if (host_nameInput.text == "") 
        {
            WrongInformation("ERROR: No lobby name");
            return false;
        }

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

    async Task<Lobby> CreateLobbyWithHeartbeatAsync()
    {
        string lobbyName = host_nameInput.text;
        int maxPlayers = 4;
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = true;

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        // Heartbeat the lobby every 15 seconds.
        StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

        Debug.Log(lobby.LobbyCode);

        return lobby;
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        yield return delay;

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }


    /*private void NetworkManagerStartHost(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= NetworkManagerStartHost;
    }*/
}
