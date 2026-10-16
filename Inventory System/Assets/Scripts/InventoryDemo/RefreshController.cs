using System.Collections.Generic;
using UnityEngine;

namespace InventoryDemo
{
    public sealed class RefreshController
    {
        private readonly InventorySession session;
        private readonly InventoryCatalog catalog;
        private readonly InventoryController controller;

        public RefreshController(InventorySession inventorySession, 
            InventoryCatalog inventoryCatalog, InventoryController inventoryController)
        {
            session = inventorySession;
            catalog = inventoryCatalog;
            controller = inventoryController;
        }

        public bool Refresh()
        {
            IReadOnlyList<ItemData> items = catalog != null ? catalog.Items : null;
            if (items == null || items.Count == 0)
            {
                controller.Notify("No items are configured in the catalog.");
                return false;
            }

            session.Refresh.Clear();
            while (HasEmptySlot(session.Refresh))
            {
                ItemData item = ChooseWeightedItem(items);
                if (item == null || !session.Refresh.TryAdd(item, 1))
                {
                    break;
                }
            }

            controller.Notify("Refresh area updated. Drag items into the backpack.");
            return true;
        }

        private static bool HasEmptySlot(InventoryModel model)
        {
            foreach (ItemSlot slot in model.Slots)
            {
                if (slot.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        private static ItemData ChooseWeightedItem(IReadOnlyList<ItemData> items)
        {
            int totalWeight = 0;
            foreach (ItemData item in items)
            {
                if (item != null)
                {
                    totalWeight += item.SpawnWeight;
                }
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);
            foreach (ItemData item in items)
            {
                if (item == null)
                {
                    continue;
                }

                roll -= item.SpawnWeight;
                if (roll < 0)
                {
                    return item;
                }
            }

            return items[0];
        }
    }
}
