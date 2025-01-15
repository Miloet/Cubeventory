using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;
using Unity.VisualScripting;

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

    async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await SignIn();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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

        try
        {
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(player_joinCode.text);
            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            WrongInformation();
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
            //NetworkManager.Singleton.SceneManager.OnLoadComplete += NetworkManagerStartHost;
            //NetworkManager.Singleton.SceneManager.LoadScene("HeadScene", LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            WrongInformation();
        }
    }

    

    public bool Player_CheckInfo()
    {
        if (player_nameInput.text != "") player_name = player_nameInput.text;
        else
        {
            WrongInformation();
            return false;
        }

        if (player_colorInput.color != Color.white) player_color = player_colorInput.color;
        else
        {
            WrongInformation();
            return false;
        }

        return true;
    }
    public bool Host_CheckInfo()
    {
        if(host_isPlayer && !Player_CheckInfo())
        {
            WrongInformation();
            return false;
        }

        if (host_nameInput.text != "") player_name = host_nameInput.text;
        else
        {
            WrongInformation();
            return false;
        }

        return true;
    }

    public void WrongInformation()
    {
        thunderAudioSource.Play();
        skeletonAnimator.SetTrigger("WrongInfo");

        playerButton.interactable = true;
        hostButton.interactable = true;
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
