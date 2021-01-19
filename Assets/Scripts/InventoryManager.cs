using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Item Pool")]
    public ItemType[] ItemPool;

    ItemType[] Inventory = new ItemType[5];

    public Text invText;

    public Image[] invGUIImages = new Image[5];

    // Update is called once per frame
    void Update()
    {
        // used for development, not quite necessary anymore but keeping just in case
        /*invText.text = "Inventory: \r\n";
        foreach(ItemType item in Inventory) { if (item == null) { invText.text += "null\r\n"; } else { invText.text += item.Name + "\r\n"; }; };*/

        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == null) 
            {
                invGUIImages[i].sprite = null;
                invGUIImages[i].color = new Color(255, 255, 255, 0f);
            } else 
            { 
                invGUIImages[i].sprite = Inventory[i].InventoryIcon;
                invGUIImages[i].color = Inventory[i].InventoryIconColour;
            }
        }

    }

    public int GetAvailableSpace()
    {
        int availableSpace = 0;
        foreach(ItemType i in Inventory) { if(i == null) { availableSpace++; }; };
        return availableSpace;
    }

    public void AddItem(ItemType i)
    {
        if (GetAvailableSpace() != 0)
        {
            int iteration = 0;
            foreach (ItemType it in Inventory)
            {
                if (it == null)
                {
                    Inventory[iteration] = i; // add item to the inventory array
                    return;
                }
                iteration++;
            }
        }
        else
        {
            throw new NoInventorySpaceException();
        }
    }

    public void AddItem(string name)
    {
        if (GetAvailableSpace() != 0)
        {
            foreach (ItemType i in ItemPool)
            {
                if (name.ToLower() == i.Name.ToLower())
                {
                    // names are matched - find the first slot that is available n bam plop it in there
                    int iteration = 0;
                    foreach (ItemType it in Inventory)
                    {
                        if (it == null)
                        {
                            Inventory[iteration] = i; // add item to the inventory array
                            break;
                        }
                        iteration++;
                    }
                    return;
                }
            }
            throw new ItemNotFoundException(name);
        }
        else
        {
            throw new NoInventorySpaceException();
        }
    }

    public bool Contains(ItemType item)
    {
        throw new NotImplementedException();
    }

    public bool Contains(InteractableObject.itemType type)
    {
        foreach(ItemType i in Inventory)
        {
            try
            {
                if (i.Type == type)
                {
                    return true;
                }
            }
            catch (NullReferenceException e)
            {
                /*Debug.Log(e.Message);*/
            }
        }
        return false;
    }

    public void RemoveItem(InteractableObject.itemType type)
    {
        for(int i = 0; i < Inventory.Length; i++)
        {
            try
            {
                if (Inventory[i].Type == type)
                {
                    // remove this item
                    Inventory[i] = null;
                }
            }
            catch(NullReferenceException e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}

[Serializable]
public class ItemType
{
    public string Name;
    public Sprite InventoryIcon;
    public Color InventoryIconColour;
    public InteractableObject.itemType Type;
    public GameObject Prefab;
}

[Serializable]
public class NoInventorySpaceException : Exception
{
    public NoInventorySpaceException() { }
    public NoInventorySpaceException(string message) { }
}

[Serializable]
public class ItemNotFoundException : Exception
{
    public ItemNotFoundException() { }
    public ItemNotFoundException(string item) { }
}
