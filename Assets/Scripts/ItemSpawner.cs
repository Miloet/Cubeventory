using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
public class ItemSpawner : MonoBehaviour
{
    public GameObject item;


    public void SpawnItem()
    {
        var g = Instantiate(item);
        NetworkObject obj = g.GetComponent<NetworkObject>();
        obj.Spawn();
        obj.ChangeOwnership(MouseBehaviour.instance.PlayerID);
        obj.TrySetParent(MouseBehaviour.instance.canvas.transform);

        var i = g.GetComponent<Item>();
        i.SendItemServerRPC("ITEM", "DESCRIPTION", Item.WeightCube(2, 2), 2, Color.white);
    }
}
