using TMPro;
using UnityEngine;

public class GetLobbyCode : MonoBehaviour
{
    private void Awake()
    {
        var network = FindFirstObjectByType<MyNetwork>();   
        
        GetComponent<TextMeshProUGUI>().text = "Lobby Code: " + network.lobby.LobbyCode;
    }
}
