using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private const int MAX_SLOTS = 5;
    private Item[] items;
    
    public event Action<int, Item> OnItemAdded; // Slot index, Item
    public event Action<int> OnItemRemoved; // Slot index

    private int selectedItemIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        items = new Item[MAX_SLOTS];
    }

    public bool AddItem(Item item)
    {
        //Debug.Log($"Attempting to add item: {(item != null ? item.GetItemName() : "null")}");
        //Debug.Log($"Current inventory state:");
        for (int i = 0; i < items.Length; i++)
        {
            //Debug.Log($"Slot {i}: {(items[i] != null ? items[i].GetItemName() : "empty")}");
        }
        
        if (item == null)
        {
            Debug.LogError("Tried to add null item to inventory");
            return false;
        }

        // Find first empty slot
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                //Debug.Log($"Successfully added {item.GetItemName()} to inventory slot {i}");
                OnItemAdded?.Invoke(i, item);
                return true;
            }
        }
        
//        Debug.Log("Inventory is full!");
        return false;
    }

    public Item GetItem(int slot)
    {
        if (slot >= 0 && slot < items.Length)
        {
            return items[slot];
        }
        return null;
    }

    public bool HasEmptySlot()
    {
        bool hasEmpty = false;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                hasEmpty = true;
                break;
            }
        }
        //Debug.Log($"HasEmptySlot check result: {hasEmpty}. Current inventory state:");
        for (int i = 0; i < items.Length; i++)
        {
            //Debug.Log($"Slot {i}: {(items[i] != null ? items[i].GetItemName() : "empty")}");
        }
        return hasEmpty;
    }

    public int GetItemCount()
    {
        int count = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                count++;
        }
        return count;
    }

    public void RemoveItem(int slot)
    {
        //Debug.Log($"Attempting to remove item from slot {slot}");
        if (slot >= 0 && slot < items.Length && items[slot] != null)
        {
            //Debug.Log($"Removing item: {items[slot].GetItemName()}");
            items[slot] = null;
            OnItemRemoved?.Invoke(slot);
            //Debug.Log("Item removed and event invoked");
        }
        //else
        //{
        //    Debug.LogError($"Invalid slot or no item in slot {slot}");
        //}
    }

    public int GetSlotForItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                return i;
            }
        }
        return -1;
    }

    public void DebugInventoryState()
    {
        //Debug.Log("=== Inventory State ===");
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                //Debug.Log($"Slot {i}: {items[i].GetItemName()} (Active: {items[i].gameObject.activeInHierarchy})");
            }
            else
            {
                //Debug.Log($"Slot {i}: empty");
            }
        }
        //Debug.Log("====================");
    }

    public Item GetEquippedItem()
    {
        if (selectedItemIndex >= 0 && selectedItemIndex < items.Length)
        {
            return items[selectedItemIndex];
        }
        return null;
    }

    public void SetSelectedItem(int index)
    {
        if (index >= -1 && index < items.Length)  // -1 is valid for "nothing selected"
        {
            selectedItemIndex = index;
        }
    }

    public Item[] GetAllItems()
    {
        return items;
    }
} 