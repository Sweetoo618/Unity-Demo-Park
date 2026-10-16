using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public ItemData itemData;
    public int stackCount;
    
    public bool IsEmpty => itemData == null || stackCount <= 0;
    
    public ItemSlot()
    {
        itemData = null;
        stackCount = 0;
    }
    
    public ItemSlot(ItemSlot other)
    {
        if (other != null)
        {
            itemData = other.itemData;
            stackCount = other.stackCount;
        }
    }
    
    public bool CanStackWith(ItemSlot other)
    {
        if (IsEmpty || other.IsEmpty) return false;
        return itemData == other.itemData || itemData.ItemId == other.itemData.ItemId;
    }
    
    public bool CanAddToStack(int amount)
    {
        if (IsEmpty) return false;
        return stackCount + amount <= itemData.MaxStackSize;
    }
    
    public void Clear()
    {
        itemData = null;
        stackCount = 0;
    }
    
    public void SetItem(ItemData data, int count = 1)
    {
        if (data == null || count <= 0)
        {
            Clear();
            return;
        }

        itemData = data;
        stackCount = Mathf.Clamp(count, 1, Mathf.Max(1, data.MaxStackSize));
    }
    
    public void AddToStack(int amount)
    {
        if (itemData != null)
        {
            stackCount = Mathf.Min(stackCount + amount, itemData.MaxStackSize);
        }
    }
    
    public void RemoveFromStack(int amount)
    {
        stackCount -= amount;
        if (stackCount <= 0) Clear();
    }
}
