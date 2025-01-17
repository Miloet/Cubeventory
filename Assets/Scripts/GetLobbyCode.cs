using TMPro;
using UnityEngine;

public class GetLobbyCode : MonoBehaviour
{
    string lobbyCode;

    private void Awake()
    {
        var network = FindFirstObjectByType<MyNetwork>();
        lobbyCode = network.lobby.LobbyCode;
        GetComponent<TextMeshProUGUI>().text = "Lobby Code: " + lobbyCode;
    }


    public void SetLobbyCodeToClip()
    {
        GUIUtility.systemCopyBuffer = lobbyCode;
    }
}
