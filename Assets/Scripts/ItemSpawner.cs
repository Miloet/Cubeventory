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
        if (Item.lastPickedUp != null)
        {
            var item = Item.lastPickedUp.gameObject;
            SetItem(item);
        }
    }

    public void RotateItem(bool forward)
    {
        return;
        if (Item.lastPickedUp != null)
        {
            var item = Item.lastPickedUp;

            bool[,] weight = item.weight;

            //FOR WHATEVER FUCK-ASS REASON THIS FUNCTION HAS TO BE CALLED 5 TIMES IN A ROW TO PROPPELRY ROTATE IT??????
            //whatever, just dont question it
            //nevermind, it just doenst work :P
            for(int i = 0; i < 5; i++)
            {
                weight = RotateMatrixCounterClockwise(weight);

                /*if (!(i + 1 < 5)) break;

                int width = weight.GetLength(0);
                int height = weight.GetLength(1);

                bool[] flatWeight = Flatten2D(weight);

                bool[,] cells = new bool[width, height];
                for (int j = 0; j < width * height; j++)
                {
                    cells[j % width, j / width] = flatWeight[j];
                }
                weight = cells;*/
            }
            //weight = RotateMatrixCounterClockwise(weight);



            item.SendItemServerRPC(item.name, item.description, item.link, GetRealWeight(weight), 
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
        item.SendItemServerRPC(nameField.text, descriptionField.text, linkField.text, GetRealWeight(weight, out width, out height), width, height, c);
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
