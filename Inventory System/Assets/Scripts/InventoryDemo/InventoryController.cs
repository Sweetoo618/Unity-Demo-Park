using System;

namespace InventoryDemo
{
    public sealed class InventoryController
    {
        private readonly InventorySession session;

        public event Action<string> Feedback;
        public InventorySession Session => session;

        public InventoryController(InventorySession inventorySession)
        {
            session = inventorySession ?? throw new ArgumentNullException(nameof(inventorySession));
        }

        public bool Add(InventoryArea area, ItemData item, int amount)
        {
            return session.Get(area).TryAdd(item, amount);
        }

        public bool Drop(InventoryArea area, int index)
        {
            InventoryModel model = session.Get(area);
            if (!TryGetOccupiedSlot(model, index, out ItemSlot slot))
            {
                return false;
            }

            string itemName = slot.itemData.ItemName;
            bool removed = model.RemoveAt(index, slot.stackCount);
            if (removed)
            {
                Notify("Dropped " + itemName + ".");
            }

            return removed;
        }

        public bool Split(InventoryArea area, int index, int amount)
        {
            bool split = session.Get(area).Split(index, amount);
            if (split)
            {
                Notify("Split stack.");
            }

            return split;
        }

        public bool Transfer(InventoryArea fromArea, int fromIndex, InventoryArea toArea, int toIndex)
        {
            InventoryModel sourceModel = session.Get(fromArea);
            InventoryModel targetModel = session.Get(toArea);
            if (!TryGetOccupiedSlot(sourceModel, fromIndex, out ItemSlot source))
            {
                return false;
            }

            if (ReferenceEquals(sourceModel, targetModel))
            {
                return sourceModel.Move(fromIndex, toIndex);
            }

            ItemData item = source.itemData;
            int amount = source.stackCount;
            bool placed;
            if (toIndex >= 0 && toIndex < targetModel.Capacity)
            {
                ItemSlot target = targetModel.Slots[toIndex];
                placed = target.IsEmpty
                    ? targetModel.TryPlaceAt(toIndex, item, amount)
                    : targetModel.TryStackAt(toIndex, item, amount);
            }
            else
            {
                placed = targetModel.TryAdd(item, amount);
            }

            if (!placed || !sourceModel.RemoveAt(fromIndex, amount))
            {
                return false;
            }

            Notify("Moved " + item.ItemName + ".");
            return true;
        }

        public ItemSlot Get(InventoryArea area, int index)
        {
            InventoryModel model = session.Get(area);
            return index >= 0 && index < model.Capacity ? model.Slots[index] : new ItemSlot();
        }

        public void Notify(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Feedback?.Invoke(message);
            }
        }

        private static bool TryGetOccupiedSlot(InventoryModel model, int index, out ItemSlot slot)
        {
            slot = null;
            if (index < 0 || index >= model.Capacity)
            {
                return false;
            }

            slot = model.Slots[index];
            return !slot.IsEmpty;
        }
    }
}
