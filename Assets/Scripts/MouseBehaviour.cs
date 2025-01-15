using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class MouseBehaviour : NetworkBehaviour
{

    public static MouseBehaviour instance;

    public string playerName;
    public ulong PlayerID;
    public Canvas canvas;
    public Transform hand;

    public Color playerColor;

    private void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        if (IsServer)
        {
            var can = FindFirstObjectByType<Canvas>();
            if (can != null)
            {
                NetworkObject.TrySetParent(can.transform, true);
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


            GetComponent<Image>().enabled = false;
            
            if(IsServer && MyNetwork.host_isPlayer) RequestInventoryServerRPC(NetworkObject.OwnerClientId);
            if(!IsServer) RequestInventoryServerRPC(NetworkObject.OwnerClientId);

            playerName = MyNetwork.player_name;
            playerColor = MyNetwork.player_color;

            //playerColor = new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f),1);
        }
        
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
