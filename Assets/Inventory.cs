using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Inventory : NetworkBehaviour
{
    public int strScore = 15;
    public static GameObject row;

    public static HashSet<Inventory> inventories = new HashSet<Inventory>();
    [System.NonSerialized] public RectTransform rectTransform;


    private void Start()
    {
        if (row == null)
        {
            row = Resources.Load<GameObject>("InventoryRow");
        }
        
        rectTransform = GetComponent<RectTransform>();
        inventories.Add(this);

        if (!IsOwner)
            RequestStrServerRPC();
        else
            SendStrServerRPC(strScore);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        inventories.Remove(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestStrServerRPC()
    {
        RequestStrClientRPC();
    }
    [ClientRpc]
    public void RequestStrClientRPC()
    {
        if(IsOwner)
            SendStrServerRPC(strScore);
    }


    [ServerRpc(RequireOwnership = true)]
    public void SendStrServerRPC(int str)
    {
        SendStrClientRPC(str);
    }
    [ClientRpc]
    public void SendStrClientRPC(int str)
    {
        UpdateInventorySpace(str);
    }

    public void UpdateInventorySpace(int str)
    {
        strScore = Mathf.Clamp(str, 1, 30);

        if (transform.childCount == strScore) return;

        int original = transform.childCount;
        if (transform.childCount > strScore)
        {
            
            for (int i = 0; i < original - strScore; i++)
            {
                Destroy(transform.GetChild(i));
            }
        }
        else
        {
            for (int i = 0; i < strScore - original; i++)
            {
                Instantiate(row, transform);
            }
        }
    }


    public ulong GetOwner()
    {
        return OwnerClientId;
    }

}
