using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;

public class BoxColor : NetworkBehaviour
{
    Material material;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("trigger");
        NetworkBehaviour networkBehaviour = other.GetComponent<NetworkBehaviour>();
        if(networkBehaviour != null && networkBehaviour.IsOwner)
        {
            print("try send");
            SendColorServerRPC(networkBehaviour.IsHost ? Color.red : Color.blue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendColorServerRPC(Color color)
    {
        SendColorClientRPC(color);
    }

    [ClientRpc]
    public void SendColorClientRPC(Color color)
    {
        color.a = 0.4f;
        material.color = color;
        print("recive");
    }
}
