using UnityEngine;

namespace InventoryDemo
{
    public sealed class InventoryInteractionController
    {
        private readonly InventoryController controller;
        private InventoryArea sourceArea;
        private int sourceIndex = -1;

        public InventoryInteractionController(InventoryController inventoryController)
        {
            controller = inventoryController;
        }

        public void BeginDrag(InventoryArea area, int index)
        {
            sourceArea = area;
            sourceIndex = index;
        }

        public bool DropOn(InventoryArea area, int index)
        {
            if (sourceIndex < 0) return false;
            bool moved = controller.Transfer(sourceArea, sourceIndex, area, index);
            ClearDrag();
            return moved;
        }

        public bool DropOnArea(InventoryArea area)
        {
            if (sourceIndex < 0) return false;
            bool moved = controller.Transfer(sourceArea, sourceIndex, area, -1);
            ClearDrag();
            return moved;
        }

        private void ClearDrag()
        {
            sourceArea = InventoryArea.Backpack;
            sourceIndex = -1;
        }
    }
}
