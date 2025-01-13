using Unity.Netcode;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int strScore = 15;
    public static GameObject row;

    private void Start()
    {
        if(row == null)
        {
            row = Resources.Load<GameObject>("InventoryRow");
        }
        UpdateInventorySpace(strScore);
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


}
