using NUnit.Framework.Internal;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ChatSystem : NetworkBehaviour
{
    private static ChatSystem instance;
    public Transform chatWindow;
    public GameObject messagePrefab;
    public GameObject imagePrefab;
    public TMP_InputField chatInput;
    public ScrollRect scrollRect;

    public bool isInMain;
    public static bool chatLeft;

    public RectTransform chatbox;

    public GameObject leftCollapseButton;
    public GameObject rightCollapseButton;

    public GameObject expandButton;

    private Vector2 closedPosition;
    private Vector2 openPosition;

    private bool isOpen = false;
    private bool isExpanded = false;
    float height = 150;

    void Start()
    {
        instance = this;
        try
        {
            OnJoinServerRPC(MyNetwork.player_name);
        }
        catch
        { }

        if(isInMain)
        {
            openPosition =  new Vector2(chatLeft ? -800 : 800, 200);
            closedPosition =new Vector2(chatLeft ? -1130 : 1130, 200);

            rightCollapseButton.SetActive(chatLeft);
            leftCollapseButton.SetActive(!chatLeft);
        }

    }

    private void Update()
    {
        if (!isInMain)
            return;

        if (!Input.GetButton("Control") && Input.GetButtonDown("Enter"))
        {
            if(chatInput.isFocused)
            {
                PlayerSendChatMessage();
            }
            if (!MouseBehaviour.isWriting())
            {
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject, null);
                chatInput.Select();
                SetOpen(true);
            }
        }

        Vector2 target = isOpen ? openPosition : closedPosition;

        float flat = 300;
        float per = 3f;

        float dist = Vector3.Distance(target, chatbox.anchoredPosition);

        chatbox.anchoredPosition = Vector3.MoveTowards(chatbox.anchoredPosition, target, Time.deltaTime * (dist*per + flat));

        height = Mathf.MoveTowards(height, isExpanded ? 400 : 150, Time.deltaTime*flat*2f);
        chatbox.sizeDelta = new Vector2(300, height);
    }

    public void SwitchOpen()
    {
        SetOpen(!isOpen);
    }
    public void SetOpen(bool open)
    {
        isOpen = open;
        rightCollapseButton.transform.rotation = isOpen ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        leftCollapseButton.transform.rotation = isOpen ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
    }
    public void SwitchExpanded()
    {
        SetExpanded(!isExpanded);
    }
    public void SetExpanded(bool Expanded)
    {
        isExpanded = Expanded;
        expandButton.transform.rotation = isExpanded ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnJoinServerRPC(string name)
    {
        OnJoinClientRPC(name);
    }

    [ClientRpc]
    public void OnJoinClientRPC(string name)
    {
        if(!IsServer)
            MyNetwork.allPlayerNames.Add(name);
  
        if(IsServer)
        {
            FindFirstObjectByType<InventorySaveSystem>().UpdatePlayerNames();
        }
    }

    #region Text Sending
    public void PlayerSendChatMessage()
    {
        if (chatInput.text == "" ||chatInput.text == "\n") return;

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
        return "" + sender + ": " + body;
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
        var g = Instantiate(instance.messagePrefab, instance.chatWindow);
        var tmp = g.GetComponent<TextMeshProUGUI>();
        tmp.text = text;

        if (duration > 0) Destroy(g, duration);

        _ = instance.ScrollToBottom();
    }
    #endregion


    #region Image Sending
    public static void SendPublicImage(Texture2D texture)
    {
        if (texture == null) return;

        if (!instance.NetworkManager.IsConnectedClient)
        {
            SystemSendMessage("youre not connected yet idiot", 5);
            return;
        }

        byte[] converted = texture.GetRawTextureData();

        instance.SendImageServerRPC(converted);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SendImageServerRPC(byte[] texture)
    {
        SendImageClientRPC(texture);
    }
    [ClientRpc]
    private void SendImageClientRPC(byte[] texture)
    {
        Texture2D tex = DrawSurface.GetTexture();
        tex.LoadRawTextureData(texture);
        tex.Apply();
        ChatSystem.CreateChatImage(tex);
    }

    private static void CreateChatImage(Texture2D texture)
    {
        var g = Instantiate(instance.imagePrefab, instance.chatWindow);
        var image = g.GetComponent<ChatDisplayImage>();
        image.AssignTexture(texture);

        _ = instance.ScrollToBottom();
    }

    #endregion 

    public async Awaitable ScrollToBottom()
    {
        await Awaitable.EndOfFrameAsync();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    public void SetIsLeft(bool left)
    {
        chatLeft = left;
    }
}
