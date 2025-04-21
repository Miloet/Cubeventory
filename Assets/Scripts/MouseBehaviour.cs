using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class MouseBehaviour : NetworkBehaviour
{

    public static MouseBehaviour instance;
    public static Camera cam;

    public string playerName;
    public ulong PlayerID;
    public Canvas canvas;
    public Transform hand;

    public Color playerColor;
    public new TMPro.TextMeshProUGUI name;

    public UnityEvent<Item> pickedUpItemEvent = new UnityEvent<Item>();
    private void Start()
    {
        cam = Camera.main;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        if (IsServer)
        {
            Transform mice = GameObject.Find("Mice").transform;
            if (mice != null)
            {
                NetworkObject.TrySetParent(mice, true);
            }
            else
            {
                Debug.LogError("Canvas not found!");
            }
        }
        if(IsOwner)
        {
            instance = this;
            PlayerID = NetworkObject.OwnerClientId;

            hand = canvas.transform.Find("Hand");


            GetComponentInChildren<Image>().enabled = false;
            GetComponentInChildren<TMPro.TextMeshProUGUI>().enabled = false;

            if (IsServer && MyNetwork.host_isPlayer) RequestInventoryServerRPC(NetworkObject.OwnerClientId);
            if(!IsServer) RequestInventoryServerRPC(NetworkObject.OwnerClientId);

            playerName = MyNetwork.player_name;
            playerColor = MyNetwork.player_color;

            SendNameServerRPC(playerName);
            //playerColor = new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f),1);
        }
        else
            RequestNameServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendNameServerRPC(string name)
    {
        SendNameClientRPC(name);
    }
    [ClientRpc(RequireOwnership = false)]
    public void SendNameClientRPC(string name)
    {
        this.name.text = name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestInventoryServerRPC(ulong uID)
    {
        RequestInventoryClientRPC(uID);
    }
    [ClientRpc(RequireOwnership = false)]
    public void RequestInventoryClientRPC(ulong uID)
    {
        if (!IsServer) return;

        CreateInventory(uID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestNameServerRPC()
    {
        RequestNameClientRPC();
    }
    [ClientRpc(RequireOwnership = false)]
    public void RequestNameClientRPC()
    {
        if (IsOwner) SendNameServerRPC(playerName);
    }


    public void CreateInventory(ulong uID)
    {
        var g = Instantiate(Resources.Load<GameObject>("Inventory"));
        var net = g.GetComponent<NetworkObject>();
        net.Spawn();
        net.TrySetParent(canvas.transform.Find("Inventories"));
        net.ChangeOwnership(uID);
    }

    void Update()
    {
        if (!IsOwner) return;

        if(Application.isFocused)
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = -1;
            transform.position = pos;
        }
    }

    public static Vector3 NormalizeMousePosition()
    {
        var normalized = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
        var position = new Vector3(normalized.x * 1920f, normalized.y * 1080f);

        return position;
    }
    public static Vector3 NormalizeMousePosition(Vector2 pos)
    {
        var normalized = new Vector3(pos.x / Screen.width, pos.y / Screen.height);
        var position = new Vector3(normalized.x * 1920f, normalized.y * 1080f);

        return position;
    }

    public static Vector2 Mouse01()
    {
        return new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
    }


    public static bool isWriting()
    {
        return EventSystem.current.currentSelectedGameObject != null;
    }
}
