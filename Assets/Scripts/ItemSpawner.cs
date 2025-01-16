using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public TMP_InputField nameField;
    public TMP_InputField descriptionField;
    public FlexibleColorPicker colorPicker;

    private Toggle[,] toggles;
    private const int maxItemSize = 6;

    private void Awake()
    {
        Toggle[,] formatted = new Toggle[maxItemSize, maxItemSize]; 
        Toggle[] all = GetComponentsInChildren<Toggle>();
        for (int i = 0; i < all.Length; i++)
        {
            formatted[i % maxItemSize, i / maxItemSize] = all[i];
        }
        toggles = formatted;
    }

    public void SpawnItem()
    {
        bool notAllEmpty = false;
        foreach(Toggle t in toggles)
        {
            if(t.isOn) 
            {
                notAllEmpty = true;
                break;
            }
        }

        if(!notAllEmpty) return;
        

        var g = CreateItem();
        SetItem(g);
    }

    public void EditItem()
    {
        var item = Item.lastPickedUp.gameObject;

        if(item != null)
            SetItem(item);
    }

    public GameObject CreateItem()
    {
        var g = Instantiate(itemPrefab);
        NetworkObject obj = g.GetComponent<NetworkObject>();
        obj.Spawn();
        obj.ChangeOwnership(MouseBehaviour.instance.PlayerID);
        obj.TrySetParent(MouseBehaviour.instance.canvas.transform);

        return g;
    }
    public void SetItem(GameObject g)
    {
        uint width;
        uint height;

        bool[,] weight = new bool[maxItemSize, maxItemSize];

        for (int i = 0; i < maxItemSize * maxItemSize; i++)
        {
            weight[i % maxItemSize, i / maxItemSize] = toggles[i % maxItemSize, i / maxItemSize].isOn;
        }

        var item = g.GetComponent<Item>();
        Color c = colorPicker.color;
        item.SendItemServerRPC(nameField.text, descriptionField.text, GetRealWeight(weight, out width, out height), width, height, c);
    }

    public bool[] GetRealWeight(bool[,] weight, out uint width, out uint height)
    {
        Vector2Int highest = new Vector2Int(-1, -1);
        Vector2Int lowest = new Vector2Int(int.MaxValue, int.MaxValue);

        for (int x = 0; x < maxItemSize; x++)
        {
            for (int y = 0; y < maxItemSize; y++)
            {
                if(weight[x,y])
                {
                    if (x > highest.x) highest.x = x;
                    if (x < lowest.x) lowest.x = x;
                    if (y > highest.y) highest.y = y;
                    if (y < lowest.y) lowest.y = y;
                }
            }
        }

        bool[,] shorten = new bool[1+highest.x - lowest.x, 1+highest.y - lowest.y];

        for (int x = lowest.x; x < highest.x+1; x++)
        {
            for (int y = lowest.y; y < highest.y+1; y++)
            {
                shorten[x - lowest.x, y - lowest.y] = weight[x, y];
            }
        }

        width = (uint)shorten.GetLength(0);
        height = (uint)shorten.GetLength(1);
        return Flatten2D(shorten);
    }
    public bool[] Flatten2D(bool[,] weight)
    {
        bool[] flat = new bool[weight.Length];
        int i = 0;

        for(int x = 0; x < weight.GetLength(0); x++)
            for(int y = 0; y < weight.GetLength(1); y++)
            {
                flat[i] = weight[x,y];
                i++;
            }

        return flat;
    }


    public void DeleteItem()
    {
        if(Item.lastPickedUp != null)
        {
            var obj = Item.lastPickedUp.GetComponent<NetworkObject>();
            obj.Despawn(true);
        }


    }
}
