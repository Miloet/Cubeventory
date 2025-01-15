using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    void Start()
    {
        Debug.Log("Start");

        /*if (IsServer)
            CreatePlayer(OwnerClientId);
        else
            RequestPlayerServerRPC(OwnerClientId);*/
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerServerRPC(ulong uId)
    {
        RequestPlayerClientRPC(uId);
    }
    [ClientRpc]
    public void RequestPlayerClientRPC(ulong uId)
    {
        Debug.Log("Player Join");

        if (IsServer)
            CreatePlayer(OwnerClientId);
    }

    public void CreatePlayer(ulong uId)
    {
        var g = Instantiate(Resources.Load<GameObject>("Mouse"));
        NetworkObject obj = g.GetComponent<NetworkObject>();
        obj.Spawn();
        obj.ChangeOwnership(uId);
    }
}
