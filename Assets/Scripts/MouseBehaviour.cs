using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MouseBehaviour : NetworkBehaviour
{

    public static ulong PlayerID;
    public static Canvas canvas;
    public static Transform hand;

    private void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        if (IsServer)
        {
            if (canvas != null)
            {
                NetworkObject.TrySetParent(canvas.transform, true);
            }
            else
            {
                Debug.LogError("Canvas not found!");
            }
        }
        if(IsOwner)
        {
            PlayerID = NetworkObject.OwnerClientId;
            GetComponent<Image>().enabled = false;
            hand = canvas.transform.Find("Hand");
            RequestInventoryServerRPC(NetworkObject.OwnerClientId);
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
