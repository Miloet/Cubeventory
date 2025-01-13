using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    
    public string name;
    public string description;
    public Color color;

    public bool[,] weight = new bool[1, 1];
    private Image[] grid;
    private Transform gridObj;

    public Animator animator;

    private TextMeshProUGUI nameText;

    public static GameObject itemCell;
    private bool hasBeenDefined;

    private bool isDragging;
    private Vector2 position;

    private void Start()
    {
        if(!hasBeenDefined)
        {
            RequestItemFromServerRPC();
        }
    }

    private void Awake()
    {
        if(itemCell == null)
            itemCell = Resources.Load<GameObject>("ItemCell");

        gridObj = transform.Find("Grid");
        nameText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void DefineItem(string name,string description, bool[] weight, uint width, Color color)
    {
        hasBeenDefined = true;
        this.name = name;
        this.description = description;

        bool[,] cells = new bool[width, weight.Length / width];
        for (int i = 0; i < weight.Length; i++)
        {
            cells[i % width, i / width] = weight[i];

            print($"checked [{i % width},{i / width}]");
        }
        this.weight = cells;
        this.color = color;


        UpdateItem();
    }

    public void UpdateItem()
    {
        nameText.text = name;
        float totalCells = weight.Length;// * weight.GetLength(1);
        


        if(gridObj.childCount < totalCells)
        {
            int original = gridObj.childCount;
            for (int i = 0; i < totalCells - original; i++)
            {
                Instantiate(itemCell, gridObj);
            }
        }

        if (gridObj.childCount > totalCells)
        {
            int original = gridObj.childCount;
            for (int i = 0; i < original - totalCells; i++)
            {
                Destroy(transform.GetChild(i));
            }
        }
        
        grid = gridObj.GetComponentsInChildren<Image>();


        for (int i = 0; i < weight.Length; i++)
        {
            grid[i].enabled = weight[i%weight.GetLength(0), i/weight.GetLength(0)];
            grid[i].color = color;
        }

        gridObj.GetComponent<GridLayoutGroup>().constraintCount = weight.GetLength(0);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SendItemServerRPC(string name, string description, bool[] weight, uint width, Color color)
    {
        SendItemClientRPC(name, description, weight, width, color);
    }
    [ClientRpc]
    public void SendItemClientRPC(string name, string description, bool[] weight, uint width, Color color)
    {
        DefineItem(name, description, weight, width, color);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestItemFromServerRPC()
    {
        RequestItemFromClientRPC();
    }
    [ClientRpc]
    public void RequestItemFromClientRPC()
    {
        if (IsServer)
            SendItemServerRPC(name, description, Convert(weight), (uint)weight.GetLength(0), color);
    }

    public int GetWeight()
    {
        return Mathf.Max(weight.GetLength(0) * weight.GetLength(1), 1);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!IsOwner) return;

        isDragging = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!IsOwner) return;

        position = eventData.position;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsOwner) return;

        animator.SetBool("Hover", false);
        isDragging = false;


        //check overlap with all inventories

        //normalize position, then round position down to 0 - 15 and check if the two fit
    }

    private void Update()
    {
        if(isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, position, 1f / GetWeight());
        }
    }


    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (IsOwner)
        {
            if(!isDragging) animator.SetBool("Hover", true);
        }
        else animator.SetTrigger("Deny");
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (IsOwner && !isDragging)
        {
            animator.SetBool("Hover", false);
        }
    }


    public static bool[] WeightCube(uint x, uint y)
    {
        bool[] weight = new bool[x*y];
        for (int i = 0; i < x*y; i++)
        {
            weight[i] = true;
        }
        return weight;
    }

    public static bool[] Convert(bool[,] data)
    {
        int width = data.GetLength(0);
        bool[] result = new bool[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = data[i%width, i/width];
        }
        return result;
    }
}
