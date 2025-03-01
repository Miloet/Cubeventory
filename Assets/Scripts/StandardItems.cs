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
    public TMP_Dropdown itemDropdown;
    public TMP_Dropdown catagoryDropdown;

    public AllStandardItem items;

    private void Start()
    {
        Load();
        contents.text = "";

        catagoryDropdown.ClearOptions();
        List<string> nameList = System.Enum.GetNames(typeof(AllStandardItem.ItemCatagory)).Select(x => ConvertEnum(x)).ToList();
        catagoryDropdown.AddOptions(nameList);
        UpdateItems();
    }

    public static string ConvertEnum(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        string spacedString = System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        System.Globalization.TextInfo textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(spacedString);
    }

    public void UpdateItems()
    {
        items.SwitchCatagory((AllStandardItem.ItemCatagory)catagoryDropdown.value);

        itemDropdown.ClearOptions();
        List<string> options = items.GetCurrentItemList().Select(i => i.name).ToList();
        itemDropdown.AddOptions(options);

        SelectItem();
    }

    public void Save()
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
        items = ((items == null) ? new AllStandardItem() : items);
    }


    public void SpawnItem()
    {
        string text = "";
        var standardItem = items.GetCurrentItemList()[itemDropdown.value];

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
        if (items.GetCurrentItemList().Length <= 0)
        {
            contents.text = "";
            return;
        }


        var item = items.GetCurrentItemList()[itemDropdown.value];

        foreach(StandardItemData i in item.items)
        {
            if(i.amount <= 0) continue;
            text += $"{i.amount} x {i.item.name}\n";
        }

        contents.text = text;
    }

    [ContextMenu("Convert Inventory to Standard Item")]
    public void ConvertInventoryToStandardItem()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("Items to convert", fullPath, "cubeventory", true);

        if (path == null)
        {
            ChatSystem.SystemSendMessage("<color=red>you fucko-! YOU DIDNT SELECT A FILE!!!! >:[");

            return;
        }

        List<StandardItem> list = new List<StandardItem>();
        foreach (var p in path)
        {
            string file = File.ReadAllText(p);
            InventoryData inventory = JsonUtility.FromJson<InventoryData>(file);
            List<ItemData> inventoryItems = inventory.itemData.ToList();
            List<StandardItemData> data = new();
            while (inventoryItems.Count > 0)
            {
                List<ItemData> sameinventoryItems = inventoryItems.Where(t => t.name == inventoryItems[0].name).ToList();

                data.Add(new StandardItemData(inventoryItems[0], sameinventoryItems.Count));
                sameinventoryItems.ForEach(item => inventoryItems.Remove(item));
            }

            if (data.Count < 1) continue;

            var altName = Path.GetFileName(p).Replace(".cubeventory","");

            var name = data.Count == 1 ? data[0].item.name : altName;

            list.Add(new StandardItem(name, data.ToArray()));
        }
        items.SetItems(list.ToArray());
        Save();
    }


}

[Serializable]
public class AllStandardItem
{
    //public StandardItem[] allItems = new StandardItem[0];
    [NonSerialized] public ItemCatagory catagory = ItemCatagory.gear;
    public enum ItemCatagory
    {
        gear,
        ammo,
        martialWeapons,
        simpleWeapons,
        spellFocus,
        supplies,
    }

    public StandardItem[] gear = new StandardItem[0];
    public StandardItem[] ammo = new StandardItem[0];
    public StandardItem[] martialWeapons = new StandardItem[0];
    public StandardItem[] simpleWeapons = new StandardItem[0];
    public StandardItem[] spellFocus = new StandardItem[0];
    public StandardItem[] supplies = new StandardItem[0];

    public void SwitchCatagory(ItemCatagory catagory)
    {
        this.catagory = catagory;
    }

    public StandardItem[] GetCurrentItemList()
    {
        switch (catagory)
        {
            default:
            case ItemCatagory.gear:
                return gear;
            case ItemCatagory.ammo:
                return ammo;
            case ItemCatagory.martialWeapons:
                return martialWeapons;
            case ItemCatagory.simpleWeapons:
                return simpleWeapons;
            case ItemCatagory.spellFocus:
                return spellFocus;
            case ItemCatagory.supplies:
                return supplies;
        }
    }


    public void SetItems(StandardItem[] newItems)
    {
        switch(catagory)
        {
            case ItemCatagory.gear:
                gear = newItems;
                break;
            case ItemCatagory.ammo:
                ammo = newItems;
                break;
            case ItemCatagory.martialWeapons:
                martialWeapons = newItems;
                break;
            case ItemCatagory.simpleWeapons:
                simpleWeapons = newItems;
                break;
            case ItemCatagory.spellFocus:
                spellFocus = newItems;
                break;
            case ItemCatagory.supplies:
                supplies = newItems;
                break;
        }
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

