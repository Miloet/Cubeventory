using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MouseBehaviour : NetworkBehaviour
{

    public static MouseBehaviour instance;

    public string playerName;
    public ulong PlayerID;
    public Canvas canvas;
    public Transform hand;

    public Color playerColor;
    public new TMPro.TextMeshProUGUI name;

    private void Start()
    {
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
            transform.position = Input.mousePosition;
    }
}
