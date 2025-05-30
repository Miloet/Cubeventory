using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using SFB;
using TMPro;


public class InventorySaveSystem : MonoBehaviour
{
    public ItemSpawner itemSpawner;

    public TMP_Dropdown playerDropdown;

    private const string rootFolderName = "Cubeventory";
    private string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private string fullPath;

    public Toggle hasBagOfHolding;
    public TMP_InputField strScoreInput;

    //private ExtensionFilter fileTypes = new ExtensionFilter("Cubeventory File", "cubeventory");

    private void Start()
    {
        UpdatePlayerNames();
    }
    public void UpdatePlayerNames()
    {
        playerDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> data = new();
        foreach (Inventory i in Inventory.inventories)
        {
            data.Add(new TMP_Dropdown.OptionData(i.owner));
        }
        playerDropdown.AddOptions(data);

        fullPath = Path.Combine(documentsFolder, rootFolderName);
    }
    
    public void SaveInventory()
    {
        int playerIndex = playerDropdown.value;

        Inventory inv = Inventory.inventories.ToArray()[playerIndex];
        ulong playerID = inv.GetOwner();

        List<Item> itemInInventory = new List<Item>();
        foreach (Item item in Item.allInventoryItems)
        {
            if(item.RectOverlap(inv.rectTransform)) itemInInventory.Add(item);
        }

        
        var allItemData = new List<ItemData>();
        foreach (Item i in itemInInventory)
        {
            var data = new ItemData();
            data.CreateItemData(i);
            allItemData.Add(data);
        }

        InventoryData invData = new InventoryData(inv.strScore, inv.hasBagOfHolding, allItemData.ToArray());


        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        string path;
        try
        {
            path = StandaloneFileBrowser.SaveFilePanel("Save Inventory", fullPath, "inv", "cubeventory");
        }
        catch
        {
            ChatSystem.SystemSendMessage("<color=red>you fucko-! YOU DIDNT SAVE THE FILE!!!! >:[");

            return;
        }
        if (path == null)
        {
            ChatSystem.SystemSendMessage("<color=red>you fucko-! YOU DIDNT SAVE THE FILE!!!! >:[");

            return;
        }

        string contents = JsonUtility.ToJson(invData, true);

        File.WriteAllText(path, contents);
    }
    public void LoadInventory()
    {
        int playerIndex = playerDropdown.value;

        Inventory inv = Inventory.inventories.ToArray()[playerIndex];
        ulong playerID = inv.GetOwner();

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        string[] path;
        try
        {
            path = StandaloneFileBrowser.OpenFilePanel("Load Inventory", fullPath, "cubeventory", false);
        }
        catch
        {
            ChatSystem.SystemSendMessage("<color=red>you fucko-! YOU DIDNT SAVE THE FILE!!!! >:[");

            return;
        }
        if (path == null)
        {
            ChatSystem.SystemSendMessage("<color=red>you fucko-! YOU DIDNT SAVE THE FILE!!!! >:[");

            return;
        }

        string file = File.ReadAllText(path.FirstOrDefault());
        InventoryData items = JsonUtility.FromJson<InventoryData>(file);
        inv.SendInvServerRPC(items.strength, items.bagOfHolding, inv.owner);
        foreach (ItemData i in items.itemData)
        {
            var g = itemSpawner.CreateItem();

            var item = g.GetComponent<Item>();
            var net = g.GetComponent<Unity.Netcode.NetworkObject>();

            _ = i.SetItemData(item, inv, playerID);
            Item.allInventoryItems.Add(item);
        }
    }

    public void UpdateInventorySize()
    {
        int str;
        if (!int.TryParse(strScoreInput.text, out str)) return;

        bool bagOfHolding = hasBagOfHolding.isOn;
        int playerIndex = playerDropdown.value;

        Inventory inv = Inventory.inventories.ToArray()[playerIndex];

        inv.SendInvServerRPC(str, bagOfHolding, inv.owner);
    }


}

[Serializable]
public struct InventoryData
{
    public int strength;
    public bool bagOfHolding;
    public ItemData[] itemData;

    public InventoryData(int strength, bool bagOfHolding, ItemData[] itemData)
    {
        this.strength = strength;
        this.bagOfHolding = bagOfHolding;
        this.itemData = itemData;
    }
}

[Serializable]
public struct ItemData
{
    public string name;
    public string description;
    public Color color;
    public string link;
    public Vector2Int inventoryPosition;
    public Vector2Int weightSize;
    public bool[] weight;

    public void CreateItemData(Item item)
    {
        CreateItemData(item.name, item.description, item.link, item.weight, item.color, item.inventoryPosition);
    }
    public void CreateItemData(string name, string description, string link, bool[,] weight, Color color, Vector2Int inventoryPosition)
    {
        this.name = name;
        this.description = description;
        this.link = link;
        this.weight = ItemSpawner.Flatten2D(weight);
        weightSize = new Vector2Int(weight.GetLength(0), weight.GetLength(1));
        this.color = color;
        this.inventoryPosition = inventoryPosition;
    }
    public async Awaitable SetItemData(Item item, Inventory inv, ulong owner)
    {
        SetItemData(item, inv == null ? false : true);
        await Awaitable.NextFrameAsync();
        if(inv != null) item.SetPositionInInventory(inventoryPosition, inv);

        item.RequestOwnershipChangeServerRPC(owner, item.transform.position);
    }
    public void SetItemData(Item item, bool isInInventory)
    {
        item.SendItemServerRPC(name, description, link, weight, (uint)weightSize.x, (uint)weightSize.y, color, isInInventory);
    }
}
