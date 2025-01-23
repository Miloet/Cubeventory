using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public TMP_InputField nameField;
    public TMP_InputField descriptionField;
    public TMP_InputField linkField;
    public FlexibleColorPicker colorPicker;

    private Toggle[,] toggles;
    private const int maxItemSize = 6;

    private void Awake()
    {
        Toggle[,] formatted = new Toggle[maxItemSize, maxItemSize]; 
        Toggle[] all = GetComponentsInChildren<Toggle>();
        for (int i = 0; i < all.Length; i++)
        {
            formatted[i / maxItemSize, i % maxItemSize] = all[i];
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
        if (Item.lastPickedUp != null)
        {
            var item = Item.lastPickedUp.gameObject;
            SetItem(item);
        }
    }

    public void RotateItem(bool forward)
    {
        if (Item.lastPickedUp != null)
        {
            var item = Item.lastPickedUp;
            bool[,] weight = item.weight;

            if (forward)
                weight = RotateMatrixClockwise(weight);
            else
                weight = RotateMatrixCounterClockwise(weight);

            item.SendItemServerRPC(item.name, item.description, item.link, Flatten2D(weight), 
                (uint)weight.GetLength(0), (uint)weight.GetLength(1), item.color);
        }
    }

    public void DeleteItem()
    {
        if (Item.lastPickedUp != null)
        {
            var obj = Item.lastPickedUp.GetComponent<NetworkObject>();
            obj.Despawn(true);
        }
    }

    public void DeleteItem(Item item)
    {
        if (item != null)
        {
            var obj = item.GetComponent<NetworkObject>();
            obj.Despawn(true);
        }
    }

    public void DeleteAllItems()
    {
        Item[] allItems = FindObjectsByType<Item>(FindObjectsSortMode.None);

        foreach(Item i in allItems)
        {
            DeleteItem(i);
        }
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
        var item = g.GetComponent<Item>();
        uint width = (uint)item.weight.GetLength(0);
        uint height = (uint)item.weight.GetLength(1);

        bool anyActive = false;

        bool[,] weight = new bool[maxItemSize, maxItemSize];

        for (int i = 0; i < maxItemSize * maxItemSize; i++)
        {
            weight[i % maxItemSize, i / maxItemSize] = toggles[i % maxItemSize, i / maxItemSize].isOn;
            if (toggles[i % maxItemSize, i / maxItemSize].isOn) anyActive = true;
        }
        
        bool[] realWeight = Flatten2D(item.weight);

        if(anyActive)
            realWeight = GetRealWeight(weight, out width, out height);
        Color c = colorPicker.color;
        item.SendItemServerRPC(nameField.text, descriptionField.text, linkField.text, realWeight, width, height, c);
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
    public bool[] GetRealWeight(bool[,] weight)
    {
        Vector2Int highest = new Vector2Int(-1, -1);
        Vector2Int lowest = new Vector2Int(int.MaxValue, int.MaxValue);

        for (int x = 0; x < weight.GetLength(0); x++)
        {
            for (int y = 0; y < weight.GetLength(1); y++)
            {
                if (weight[x, y])
                {
                    if (x > highest.x) highest.x = x;
                    if (x < lowest.x) lowest.x = x;
                    if (y > highest.y) highest.y = y;
                    if (y < lowest.y) lowest.y = y;
                }
            }
        }

        bool[,] shorten = new bool[1 + highest.x - lowest.x, 1 + highest.y - lowest.y];

        for (int x = lowest.x; x < highest.x + 1; x++)
        {
            for (int y = lowest.y; y < highest.y + 1; y++)
            {
                shorten[x - lowest.x, y - lowest.y] = weight[x, y];
            }
        }
        return Flatten2D(shorten);
    }
    public static bool[] Flatten2D(bool[,] weight)
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
    public static bool[,] Unflatten2D(bool[] weight, int width)
    {
        int height = weight.Length / width;
        bool[,] cells = new bool[width, weight.Length / width];
        for (int i = 0; i < weight.Length; i++)
        {
            cells[i / height, i % height] = weight[i];
        }
        return cells;
    }


    static bool[,] RotateMatrixClockwise(bool[,] oldMatrix)
    {
        bool[,] newMatrix = new bool[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
        int newColumn, newRow;

        newColumn = 0;
        for (int oldRow = oldMatrix.GetLength(0) - 1; oldRow >= 0; oldRow--)
        {
            newRow = 0;
            for (int oldColumn = 0; oldColumn < oldMatrix.GetLength(1); oldColumn++)
            {
                newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
                newRow++;
            }
            newColumn++;
        }

        return newMatrix;
    }

    static bool[,] RotateMatrixCounterClockwise(bool[,] oldMatrix)
    {
        bool[,] newMatrix = new bool[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
        int newColumn, newRow = 0;
        for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
        {
            newColumn = 0;
            for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
            {
                newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
                newColumn++;
            }
            newRow++;
        }
        return newMatrix;
    }


}
