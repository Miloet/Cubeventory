using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    
    public string name;
    public string description;
    public Color color;
    public NetworkVariable<Color> outlineColor = new NetworkVariable<Color>(Color.black, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public Vector2Int inventoryPosition;
    public bool[,] weight = new bool[1, 1];
    [System.NonSerialized] public Image[] grid;
    private Transform gridObj;
    public Transform visual;
    [SerializeField] private Transform hitboxes;

    public Transform outline;
    public Outline[] outlines;
    public Image[] outlineImages;


    public Animator animator;

    private TextMeshProUGUI nameText;

    public static GameObject itemCell;
    private bool hasBeenDefined = false;

    private bool isDragging;
    private Vector2 position;

    Vector3 movementDelta;
    Vector3 rotationDelta;
    [SerializeField] private float positionLerp;
    [SerializeField] private float flatSpeed;
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;

    public static HashSet<Item> allInventoryItems = new HashSet<Item>();
    public static Item lastPickedUp;

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

        visual = transform.Find("Visual");
        gridObj = visual.Find("Animator/Grid");
        outline = visual.Find("Animator/Outline");
        nameText = GetComponentInChildren<TextMeshProUGUI>();

        outlineColor.OnValueChanged += UpdateOutlineColor;


        if (MouseBehaviour.instance != null && MouseBehaviour.instance.canvas != null) visual.SetParent(MouseBehaviour.instance.canvas.transform, true);
    }

    private void OnDisable()
    {
        outlineColor.OnValueChanged -= UpdateOutlineColor;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        allInventoryItems.Remove(this);
        if(lastPickedUp == this) lastPickedUp = null;
        Destroy(visual.gameObject);
    }


    [ServerRpc(RequireOwnership = true)]
    public void UpdateColorServerRPC(bool obstructed)
    {
        UpdateColorClientRPC(obstructed);
    }
    [ClientRpc]
    public void UpdateColorClientRPC(bool obstructed)
    {
        foreach(Image i in grid)
        {
            i.color = obstructed ? Color.red : color;
        }
    }
    public void UpdateOutlineColor(Color previous, Color current)
    {
        foreach(Outline line in outlines)
        {
            line.effectColor = current;
        }
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        base.OnOwnershipChanged(previous, current);

        if(MouseBehaviour.instance.PlayerID == current) outlineColor.Value = MouseBehaviour.instance.playerColor;
    }

    #region Create Item

    public void DefineItem(string name,string description, bool[] weight, uint width, uint height, Color color, bool inPlace = false)
    {
        if (IsOwner)
        {
            if (inPlace)
                NetworkObject.TrySetParent(MouseBehaviour.instance.canvas.transform, true);
            else
                NetworkObject.TrySetParent(MouseBehaviour.instance.hand, true);
        }
        visual.SetParent(MouseBehaviour.instance.canvas.transform, true);
        hasBeenDefined = true;
        if(name != "") this.name = name;
        if (description != "") this.description = description;

        if (weight.Length != 0)
        {
            bool[,] cells = new bool[width, height];
            for (int i = 0; i < weight.Length; i++)
            {
                cells[i % width, i / width] = weight[i];
            }
            this.weight = cells;
            
        }
        if (color.a > 0)
        {
            this.color = color;
            this.color.a = 1;
        }

        UpdateItem();

        if (IsOwner)
        {
            outlineColor.Value = MouseBehaviour.instance.playerColor;
        }
    }

    public async void UpdateItem()
    {
        nameText.text = name;
        float totalCells = weight.Length;// * weight.GetLength(1);

        if(gridObj.childCount < totalCells)
        {
            int original = gridObj.childCount;
            for (int i = 0; i < totalCells - original; i++)
            {
                Instantiate(itemCell, gridObj);
                var line = Instantiate(itemCell, outline)
                    .AddComponent<Outline>();

                line.effectDistance = new Vector2(2, -2);
            }
        }

        if (gridObj.childCount > totalCells)
        {
            int original = gridObj.childCount;
            for (int i = 0; i < original - totalCells; i++)
            {
                Destroy(gridObj.GetChild(i).gameObject);
                Destroy(outline.transform.GetChild(i).gameObject);
            }
        }
        
        grid = gridObj.GetComponentsInChildren<Image>();
        outlines = outline.GetComponentsInChildren<Outline>();
        outlineImages = outline.GetComponentsInChildren<Image>();

        for (int i = 0; i < weight.Length; i++)
        {
            bool enable = weight[i % weight.GetLength(0), i / weight.GetLength(0)];

            grid[i].enabled = enable;
            outlineImages[i].enabled = enable;
            outlines[i].effectColor = outlineColor.Value;
            grid[i].color = color;
        }

        gridObj.GetComponent<GridLayoutGroup>().constraintCount = weight.GetLength(1);
        outline.GetComponent<GridLayoutGroup>().constraintCount = weight.GetLength(1);

        await SetHitbox();
    }
    private async Awaitable SetHitbox()
    {
        foreach(Transform child in hitboxes)
        {
            Destroy(child.gameObject);
        }

        await Awaitable.NextFrameAsync();

        foreach(Image child in grid)
        {
            if (!child.isActiveAndEnabled) continue;

            var g = Instantiate(itemCell, hitboxes);
            g.transform.localPosition = child.transform.localPosition;
            var image = g.GetComponent<Image>();
            image.raycastTarget = true;
            image.color = new Color(0,0,0,0);
        }

    }


    [ServerRpc(RequireOwnership = false)]
    public void SendItemServerRPC(string name, string description, bool[] weight, uint width, uint height, Color color)
    {
        SendItemClientRPC(name, description, weight, width, height, color);
    }
    [ClientRpc]
    public void SendItemClientRPC(string name, string description, bool[] weight, uint width, uint height, Color color)
    {
        DefineItem(name, description, weight, width, height, color);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestItemFromServerRPC()
    {
        print("ITEM REQUESTED");
        RequestItemFromClientRPC();
    }
    [ClientRpc]
    public void RequestItemFromClientRPC()
    {
        if (IsServer)
            SendItemServerRPC(name, description, Convert(weight), (uint)weight.GetLength(0), (uint)weight.GetLength(1), color);
    }

    #endregion

    #region Drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!IsOwner) return;
        NetworkObject.TrySetParent(MouseBehaviour.instance.canvas.transform, true);
        isDragging = true;
        lastPickedUp = this;
        inventoryPosition = new Vector2Int(-1, -1);
        PlaceServerRPC(false);
        UpdateColorServerRPC(false);
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

        bool overlap = false;
        RectTransform inventory = null;
        Transform rows = null;
        ulong ownerId = 0;


        foreach(Inventory inv in Inventory.inventories)
        {
            var rt = inv.rectTransform;
            overlap = RectOverlap(rt);
            ownerId = inv.GetOwner();
            rows = inv.inventoryRows;
            inventory = rt;
            if (overlap) break;
        }
        if (overlap)
        {
            #region Set Position

            int inventoryCellSize = ((int)inventory.rect.width) / 15;
            RectTransform rect = ((RectTransform)transform);
            Vector2 bottemLeftSelf = rect.position - (Vector3)(rect.rect.size / 2f);
            Vector2 invRect = inventory.rect.size;
            invRect.y = rows.childCount * inventoryCellSize;
            Vector2 bottemLeftInv = inventory.position - (Vector3)(invRect / 2f);
            Vector2 diff = bottemLeftSelf - bottemLeftInv;
            Vector2Int pos = new Vector2Int(
                Mathf.RoundToInt(diff.x / inventoryCellSize),
                Mathf.RoundToInt(diff.y / inventoryCellSize));

            pos.Clamp(new Vector2Int(0, 0), new Vector2Int(15 - weight.GetLength(0), rows.childCount - weight.GetLength(1)));

            inventoryPosition = pos;

            rect.position =(rect.rect.size / 2f) + bottemLeftInv + pos * inventoryCellSize;

            #endregion

            CheckForOverlap();
            PlaceServerRPC(true);

            if (ownerId != MouseBehaviour.instance.PlayerID)
            {
                RequestOwnershipChangeServerRPC(ownerId, transform.position);
            }
        }
        else
        {
            NetworkObject.TrySetParent(MouseBehaviour.instance.hand, true);
            transform.SetAsLastSibling();
            visual.transform.SetAsLastSibling();
        }
    }

    public void CheckForOverlap()
    {
        RectTransform rect = ((RectTransform)transform);
        List<Item> semiOverlap = new();

        foreach (Item item in allInventoryItems)
        {
            if (item == this) continue;

            if (item.RectOverlap(rect))
            {
                semiOverlap.Add(item);
            }
        }

        
        foreach (Item item in semiOverlap)
        {
            if(ItemOverlap(item))
            {
                UpdateColorServerRPC(true);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    public void RequestOwnershipChangeServerRPC(ulong newOwner, Vector3 position)
    {
        RequestOwnershipChangeClientRPC(newOwner, position);
    }
    [ClientRpc]
    public void RequestOwnershipChangeClientRPC(ulong newOwner, Vector3 position)
    {
        if(IsServer)
        {
            NetworkObject.ChangeOwnership(newOwner);
        }
        transform.position = position;
    }
    [ServerRpc(RequireOwnership = false)]
    public void PlaceServerRPC(bool inInventory)
    {
        PlaceClientRPC(inInventory);
    }
    [ClientRpc]
    public void PlaceClientRPC(bool inInventory)
    {
        if(inInventory) allInventoryItems.Add(this);
        else allInventoryItems.Remove(this);
    }

    #endregion

    #region Hover

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (IsOwner)
        {
            if (!isDragging) animator.SetBool("Hover", true);
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

    #endregion

    

    private void Update()
    {
        if(isDragging)
        {
            transform.position = position;
        }

        float distance = Vector2.Distance(visual.position, transform.position) * positionLerp;
        visual.position = Vector3.MoveTowards(visual.position, transform.position, (distance + flatSpeed) * Time.deltaTime);

        if (IsOwner)
        {
            Vector3 movement = (visual.position - transform.position);
            movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
            Vector3 movementRotation = (isDragging ? movementDelta : movement) * rotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(visual.eulerAngles.x, visual.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
        }

        visual.eulerAngles = transform.eulerAngles;
    }


    private bool RectOverlap(RectTransform image2rt)
    {
        RectTransform image1rt = ((RectTransform)transform);
        Rect image1rect = image1rt.rect;
        Rect image2rect = image2rt.rect;
        if (image1rt.position.x - image1rect.width / 2f *0.95f < image2rt.position.x + image2rect.width / 2f &&
            image1rt.position.x + image1rect.width / 2f * 0.95f > image2rt.position.x - image2rect.width / 2f &&
            image1rt.position.y - image1rect.height / 2f * 0.95f < image2rt.position.y + image2rect.height / 2f &&
            image1rt.position.y + image1rect.height / 2f * 0.95f > image2rt.position.y - image2rect.height / 2f)
            return true;
        else return false;
    }

    private bool ItemOverlap(Item other)
    {
        Vector2Int end =
            new Vector2Int(
                Mathf.Min(
                    inventoryPosition.x + weight.GetLength(0), 
                    other.inventoryPosition.x + other.weight.GetLength(0)),

                Mathf.Min(
                    inventoryPosition.y + weight.GetLength(1),
                    other.inventoryPosition.y + other.weight.GetLength(1))
            );
        Vector2Int start =
            new Vector2Int(
                Mathf.Max(
                    inventoryPosition.x,
                    other.inventoryPosition.x),

                Mathf.Max(
                    inventoryPosition.y,
                    other.inventoryPosition.y)
            );


        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                if (weight[x - inventoryPosition.x,  y - inventoryPosition.y] &&
                    other.weight[x - other.inventoryPosition.x, y - other.inventoryPosition.y])
                {

                    return true;
                }
            }
        }
        
        return false;
    }

    public int GetWeight()
    {
        return Mathf.Max(weight.GetLength(0) * weight.GetLength(1), 1);
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
