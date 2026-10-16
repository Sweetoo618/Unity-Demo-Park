using UnityEngine;

namespace InventoryDemo
{
    public sealed class CraftingController
    {
        private readonly InventorySession session;
        private readonly InventoryCatalog catalog;
        private readonly InventoryController controller;

        public CraftingController(InventorySession inventorySession, InventoryCatalog inventoryCatalog,
            InventoryController inventoryController)
        {
            session = inventorySession;
            catalog = inventoryCatalog;
            controller = inventoryController;
        }

        public bool TryCraft()
        {
            ForgingRecipe recipe = FindMatchingRecipe();
            if (recipe == null)
            {
                controller.Notify("No recipe matches the forge input.");
                return false;
            }

            if (recipe.OutputItem == null || !session.ForgeOutput.CanAdd(recipe.OutputItem, recipe.OutputCount))
            {
                controller.Notify("Forge output has no room.");
                return false;
            }

            Consume(recipe);
            session.ForgeOutput.TryAdd(recipe.OutputItem, recipe.OutputCount);
            controller.Notify("Crafted " + recipe.OutputItem.ItemName + ".");
            return true;
        }

        private ForgingRecipe FindMatchingRecipe()
        {
            if (catalog == null || catalog.Recipes == null)
            {
                return null;
            }

            foreach (ForgingRecipe recipe in catalog.Recipes)
            {
                if (recipe != null && recipe.Matches(session.ForgeInput.Slots))
                {
                    return recipe;
                }
            }

            return null;
        }

        private void Consume(ForgingRecipe recipe)
        {
            foreach (ItemRequirement requirement in recipe.InputItems)
            {
                if (requirement == null || requirement.ItemData == null || requirement.Count <= 0)
                {
                    continue;
                }

                int remaining = requirement.Count;
                for (int i = 0; i < session.ForgeInput.Capacity && remaining > 0; i++)
                {
                    ItemSlot slot = session.ForgeInput.Slots[i];
                    if (slot.IsEmpty || !InventoryModel.AreSameItem(slot.itemData, requirement.ItemData))
                    {
                        continue;
                    }

                    int consumed = Mathf.Min(remaining, slot.stackCount);
                    session.ForgeInput.RemoveAt(i, consumed);
                    remaining -= consumed;
                }
            }
        }
    }
}
