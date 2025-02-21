using TMPro;
using UnityEngine;
using System.IO;
using System;
using SFB;
using System.Linq;
using System.Collections.Generic;
using NUnit;
using Unity.Netcode;
public class StandardItems : MonoBehaviour
{
    private const string rootFolderName = "Cubeventory";
    private const string fileName = "StandardItems.si";
    private static string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private static string fullPath;

    public ItemSpawner itemSpawner;
    public TextMeshProUGUI contents;
    public TMP_Dropdown dropdown;

    public AllStandardItem items;

    private void Start()
    {
        Load();
        contents.text = "";

        dropdown.ClearOptions();
        List<string> options = items.allItems.Select(i => i.name).ToList();
        dropdown.AddOptions(options);
    }

    public static void Save(AllStandardItem items)
    {
        fullPath = Path.Combine(documentsFolder, rootFolderName, fileName);
        string content = JsonUtility.ToJson(items, true);
        File.WriteAllText(fullPath,content);
    }
    public void Load()
    {
        string file;
        try
        {
            fullPath = Path.Combine(documentsFolder, rootFolderName, fileName);
            file = File.ReadAllText(fullPath);
        }
        catch 
        {
            file = Resources.Load<TextAsset>("StandardItems").text;
        }
        items = JsonUtility.FromJson<AllStandardItem>(file);
        items = ((items == null) ? new AllStandardItem(new StandardItem[0]) : items);
    }

    public static AllStandardItem StaticLoad()
    {
        string file;
        try
        {
            fullPath = Path.Combine(documentsFolder, rootFolderName, fileName);
            file = File.ReadAllText(fullPath);
        }
        catch
        {
            file = Resources.Load<TextAsset>("StandardItems").text;
        }
        var items = JsonUtility.FromJson<AllStandardItem>(file);
        return ((items == null) ? new AllStandardItem(new StandardItem[0]) : items);
    }


    public void SpawnItem()
    {
        string text = "";
        var standardItem = items.allItems[dropdown.value];

        foreach (StandardItemData item in standardItem.items)
        {
            for(int n = 0; n < item.amount; n++)
            {
                var g = itemSpawner.CreateItem();

                var i = g.GetComponent<Item>();
                var net = g.GetComponent<Unity.Netcode.NetworkObject>();

                _ = item.item.SetItemData(i, null, MouseBehaviour.instance.PlayerID);
            }
        }
    }

    public void SelectItem()
    {
        string text = "";
        var item = items.allItems[dropdown.value];

        foreach(StandardItemData i in item.items)
        {
            if(i.amount <= 0) continue;
            text += $"{i.amount} x {i.item.name}\n";
        }

        contents.text = text;
    }

    [ContextMenu("Convert Inventory to Standard Item")]
    public static void ConvertInventoryToStandardItem()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("Load Inventory", fullPath, "cubeventory", false);

        if (path == null)
        {
            ChatSystem.SystemSendMessage("<color=red>you fucko-! YOU DIDNT SELECT A FILE!!!! >:[");

            return;
        }
        string file = File.ReadAllText(path.FirstOrDefault());
        InventoryData inventory = JsonUtility.FromJson<InventoryData>(file);
        List<ItemData> inventoryItems = inventory.itemData.ToList();
        List<StandardItemData> data = new();
        while(inventoryItems.Count > 0)
        {
            List<ItemData> sameinventoryItems = inventoryItems.Where(t => t.name == inventoryItems[0].name).ToList();

            data.Add(new StandardItemData(inventoryItems[0], sameinventoryItems.Count));
            sameinventoryItems.ForEach(item => inventoryItems.Remove(item));
        }

        if (data.Count < 1) return;

        var name = data.Count == 1 ? data[0].item.name : Path.GetFileName(path.FirstOrDefault());

        var load = StaticLoad();
        var list = new List<StandardItem>();
        if (load != null && load.allItems != null)
            list = load.allItems.ToList();
        list.Add(new StandardItem(name, data.ToArray()));
        var array = list.ToArray();
        Save(new AllStandardItem(array));
    }


}

[Serializable]
public class AllStandardItem
{
    public StandardItem[] allItems = new StandardItem[0];
    public AllStandardItem(StandardItem[] items)
    {
        allItems = items;
    }
}

[Serializable]
public class StandardItem
{
    public string name;
    public StandardItemData[] items;
    public StandardItem(string name, StandardItemData[] items)
    {
        this.name = name;
        this.items = items;
    }
}


[Serializable]
public class StandardItemData
{
    public int amount;
    public ItemData item;

    public StandardItemData(ItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

