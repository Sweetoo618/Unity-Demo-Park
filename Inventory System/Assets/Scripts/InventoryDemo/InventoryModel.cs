using System;
using UnityEngine;

namespace InventoryDemo
{
    public sealed class InventoryModel
    {
        private readonly ItemSlot[] slots;

        public ItemSlot[] Slots => slots;
        public int Capacity => slots.Length;
        public event Action Changed;

        public InventoryModel(int capacity)
        {
            slots = new ItemSlot[Mathf.Max(1, capacity)];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new ItemSlot();
            }
        }

        public bool TryAdd(ItemData item, int amount)
        {
            if (item == null || amount <= 0 || !CanAdd(item, amount))
            {
                return false;
            }

            int remaining = amount;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                ItemSlot slot = slots[i];
                if (slot.IsEmpty || !AreSameItem(slot.itemData, item))
                {
                    continue;
                }

                int free = Mathf.Max(0, item.MaxStackSize - slot.stackCount);
                int added = Mathf.Min(remaining, free);
                slot.AddToStack(added);
                remaining -= added;
            }

            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    continue;
                }

                int added = Mathf.Min(remaining, item.MaxStackSize);
                slots[i].SetItem(item, added);
                remaining -= added;
            }

            RaiseChanged();
            return true;
        }

        public bool CanAdd(ItemData item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return false;
            }

            int freeCapacity = 0;
            int stackSize = item.MaxStackSize;
            foreach (ItemSlot slot in slots)
            {
                if (slot.IsEmpty)
                {
                    freeCapacity += stackSize;
                }
                else if (AreSameItem(slot.itemData, item))
                {
                    freeCapacity += Mathf.Max(0, stackSize - slot.stackCount);
                }

                if (freeCapacity >= amount)
                {
                    return true;
                }
            }
            
            return false;
        }

        public bool TryPlaceAt(int index, ItemData item, int amount)
        {
            if (!IsValid(index) || item == null || amount <= 0 || !slots[index].IsEmpty ||
                amount > item.MaxStackSize)
            {
                return false;
            }

            slots[index].SetItem(item, amount);
            RaiseChanged();
            return true;
        }

        public bool TryStackAt(int index, ItemData item, int amount)
        {
            if (!IsValid(index) || item == null || amount <= 0)
            {
                return false;
            }

            ItemSlot target = slots[index];
            if (target.IsEmpty || !AreSameItem(target.itemData, item) ||
                target.stackCount + amount > item.MaxStackSize)
            {
                return false;
            }

            target.AddToStack(amount);
            RaiseChanged();
            return true;
        }

        public bool RemoveAt(int index, int amount)
        {
            if (!IsValid(index) || amount <= 0 || slots[index].IsEmpty || slots[index].stackCount < amount)
            {
                return false;
            }

            slots[index].RemoveFromStack(amount);
            RaiseChanged();
            return true;
        }

        public bool Move(int from, int to)
        {
            if (!IsValid(from) || !IsValid(to) || from == to || slots[from].IsEmpty)
            {
                return false;
            }

            ItemSlot source = slots[from];
            ItemSlot target = slots[to];
            if (!target.IsEmpty && AreSameItem(source.itemData, target.itemData))
            {
                int free = Mathf.Max(0, target.itemData.MaxStackSize - target.stackCount);
                int moved = Mathf.Min(source.stackCount, free);
                if (moved <= 0)
                {
                    return false;
                }

                target.AddToStack(moved);
                source.RemoveFromStack(moved);
            }
            else
            {
                ItemData targetItem = target.itemData;
                int targetCount = target.stackCount;
                target.SetItem(source.itemData, source.stackCount);
                source.SetItem(targetItem, targetCount);
            }

            RaiseChanged();
            return true;
        }

        public bool Split(int index, int amount)
        {
            if (!IsValid(index) || slots[index].IsEmpty || amount <= 0 || amount >= slots[index].stackCount)
            {
                return false;
            }

            int emptyIndex = Array.FindIndex(slots, slot => slot.IsEmpty);
            if (emptyIndex < 0)
            {
                return false;
            }

            ItemData item = slots[index].itemData;
            slots[index].RemoveFromStack(amount);
            slots[emptyIndex].SetItem(item, amount);
            RaiseChanged();
            return true;
        }

        public void Clear()
        {
            bool changed = false;
            foreach (ItemSlot slot in slots)
            {
                if (slot.IsEmpty)
                {
                    continue;
                }

                slot.Clear();
                changed = true;
            }

            if (changed)
            {
                RaiseChanged();
            }
        }

        private bool IsValid(int index) => index >= 0 && index < slots.Length;

        private void RaiseChanged() => Changed?.Invoke();

        internal static bool AreSameItem(ItemData first, ItemData second)
        {
            return first != null && second != null &&
                   (first == second || (!string.IsNullOrEmpty(first.ItemId) && first.ItemId == second.ItemId));
        }
    }
}
