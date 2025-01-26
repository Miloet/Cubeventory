using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Inventory : NetworkBehaviour
{
    public int strScore;
    public bool hasBagOfHolding;
    public static GameObject row;
    public static GameObject bagOfHoldingRow;

    public static HashSet<Inventory> inventories = new HashSet<Inventory>();
    [System.NonSerialized] public RectTransform rectTransform;
    public Transform inventoryRows;
    
    [System.NonSerialized] public string owner;
    public TextMeshProUGUI ownerName;


    public const int bagOfHoldingSize = 4;

    private void Start()
    {
        if (row == null)
        {
            row = Resources.Load<GameObject>("InventoryRow");
            bagOfHoldingRow = Resources.Load<GameObject>("BagOfHoldingRow");
        }
        
        rectTransform = GetComponent<RectTransform>();
        inventories.Add(this);

        if (!IsOwner)
            RequestInvServerRPC();
        else
            SendInvServerRPC(strScore = Random.Range(10, 20), false, MyNetwork.player_name);
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
            SendInvServerRPC(strScore, hasBagOfHolding, MyNetwork.player_name);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SendInvServerRPC(int str, bool hasBagOfHolding, string name)
    {
        SendInvClientRPC(str, hasBagOfHolding, name);
    }
    [ClientRpc]
    public void SendInvClientRPC(int str, bool hasBagOfHolding, string name)
    {
        owner = name;
        ownerName.text = $"{owner}'s Inventory";
        UpdateInventorySpace(str, hasBagOfHolding);
    }

    public void UpdateInventorySpace(int str, bool hasBagOfHolding)
    {
        strScore = Mathf.Clamp(str, 1, 30);


        if (inventoryRows.childCount != strScore || this.hasBagOfHolding != hasBagOfHolding)
        {
            foreach (Transform child in inventoryRows)
            {
                Destroy(child.gameObject);
            }
                
            for (int i = 0; i < strScore; i++)
            {
                Instantiate(row, inventoryRows);
            }
            if(hasBagOfHolding)
                for (int i = 0; i < bagOfHoldingSize; i++)
                {
                    Instantiate(bagOfHoldingRow, inventoryRows);
                }
            
        }

        RectTransform singleRow = ((RectTransform)row.transform);
        size = singleRow.sizeDelta * singleRow.localScale;
        this.hasBagOfHolding = hasBagOfHolding;
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
