using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ChatSystem : NetworkBehaviour
{
    private static ChatSystem instance;
    public Transform chatWindow;
    public GameObject messagePrefab;
    public TMP_InputField chatInput;
    
    void Start()
    {
        instance = this;


        try
        {
            OnJoinServerRPC(MyNetwork.player_name);
        }
        catch
        { }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnJoinServerRPC(string name)
    {
        OnJoinClientRPC(name);
    }

    [ClientRpc]
    public void OnJoinClientRPC(string name)
    {
        MyNetwork.allPlayerNames.Add(name);
  
        if(IsServer)
        {
            FindFirstObjectByType<InventorySaveSystem>().UpdatePlayerNames();
        }
    }

    public void PlayerSendChatMessage()
    {
        if (chatInput.text == "") return;
        string message = GetCompleteMessage(MyNetwork.player_name,chatInput.text);
        SendPublicChatMessage(message);
        chatInput.text = "";
    }
    public static void SystemSendMessage(string text, float duration = 0)
    {
        if (text == "") return;
        string message = GetCompleteMessage("System", text);
        SendPrivateChatMessage(message, duration);
    }

    public static string GetCompleteMessage(string sender, string body)
    {
        return "<i>" + sender + " </i>: " + body;
    }



    private void SendPublicChatMessage(string text)
    {
        if (text == "") return;

        if(!NetworkManager.IsConnectedClient)
        {
            SystemSendMessage("youre not connected yet idiot", 5);
            return;
        }

        SendMessageServerRPC(text);
    }

    private static void SendPrivateChatMessage(string text, float duration = 0)
    {
        if (text == "") return;
        CreateChatMessage(text, duration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServerRPC(string message)
    {
        SendMessageClientRPC(message);
    }
    [ClientRpc]
    private void SendMessageClientRPC(string message)
    {
        ChatSystem.CreateChatMessage(message);
    }

    private static void CreateChatMessage(string text, float duration = 0)
    {
        var g =Instantiate(instance.messagePrefab, instance.chatWindow);
        var tmp = g.GetComponent<TextMeshProUGUI>();
        tmp.text = text;

        if (duration > 0) Destroy(g, duration);
    }
}
