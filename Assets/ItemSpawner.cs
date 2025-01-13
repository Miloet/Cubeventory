using Unity.Netcode;
using UnityEngine;
public class ItemSpawner : MonoBehaviour
{
    public GameObject item;


    public void SpawnItem()
    {
        var g = Instantiate(item);
        NetworkObject obj = g.GetComponent<NetworkObject>();
        obj.Spawn();
        obj.ChangeOwnership(MouseBehaviour.PlayerID);
        obj.TrySetParent(MouseBehaviour.canvas.transform);
    }
}
