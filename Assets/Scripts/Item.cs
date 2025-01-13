using Unity.Collections;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : NetworkBehaviour, IDragHandler
{
    public bool[,] weight = new bool[0,0];
    public string name;
    public string description;
    public Color color;

    public int GetWeight()
    {
        return Mathf.Max(weight.GetLength(0) + weight.GetLength(1), 1);
    }



    public void OnDrag(PointerEventData eventData)
    {
        if(!IsOwner) return;
        transform.position = Vector3.Lerp(transform.position, eventData.position, 1f/GetWeight());
    }

    private void Update()
    { }

}
