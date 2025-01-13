using Unity.Netcode;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int strScore = 15;
    public static GameObject row;

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

        if(transform.childCount > strScore)
        {
            for (int i = 0; i < transform.childCount - strScore; i++)
            {
                Destroy(transform.GetChild(i));
            }
        }
        else
        {
            for (int i = 0; i < strScore - transform.childCount; i++)
            {
                Instantiate(row, transform);
            }
        }
    }


}
