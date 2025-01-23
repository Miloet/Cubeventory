using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Inventory : NetworkBehaviour
{
    public int strScore;
    public static GameObject row;

    public static HashSet<Inventory> inventories = new HashSet<Inventory>();
    [System.NonSerialized] public RectTransform rectTransform;
    public Transform inventoryRows;
    
    [System.NonSerialized] public string owner;
    public TextMeshProUGUI ownerName;


    private void Start()
    {
        if (row == null)
        {
            row = Resources.Load<GameObject>("InventoryRow");
        }
        
        rectTransform = GetComponent<RectTransform>();
        inventories.Add(this);

        if (!IsOwner)
            RequestInvServerRPC();
        else
            SendInvServerRPC(strScore = Random.Range(10, 20), MyNetwork.player_name);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        inventories.Remove(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestInvServerRPC()
    {
        RequestInvClientRPC();
    }
    [ClientRpc]
    public void RequestInvClientRPC()
    {
        if(IsOwner)
            SendInvServerRPC(strScore, MyNetwork.player_name);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SendInvServerRPC(int str, string name)
    {
        SendInvClientRPC(str, name);
    }
    [ClientRpc]
    public void SendInvClientRPC(int str, string name)
    {
        owner = name;
        ownerName.text = $"{owner}'s Inventory";
        UpdateInventorySpace(str);
    }

    public void UpdateInventorySpace(int str)
    {
        strScore = Mathf.Clamp(str, 1, 30);

        if (inventoryRows.childCount != strScore)
        {
            int original = inventoryRows.childCount;
            if (inventoryRows.childCount > strScore)
            {
                for (int i = 0; i < original - strScore; i++)
                {
                    Destroy(inventoryRows.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = 0; i < strScore - original; i++)
                {
                    Instantiate(row, inventoryRows);
                }
            }
        }

        RectTransform singleRow = ((RectTransform)row.transform);
        size = singleRow.sizeDelta * singleRow.localScale;

    }

    private Vector2 size;

    private void LateUpdate()
    {
        rectTransform.sizeDelta = new Vector2(size.x, size.y * inventoryRows.childCount);
    }


    public ulong GetOwner()
    {
        return OwnerClientId;
    }
}
