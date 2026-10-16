using System.Collections.Generic;
using UnityEngine;

namespace InventoryDemo
{
    [CreateAssetMenu(fileName = "Inventory Catalog", menuName = "Inventory/Demo Catalog")]
    public sealed class InventoryCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemData> items = 
            new List<ItemData>();
        [SerializeField] private List<ForgingRecipe> recipes = 
            new List<ForgingRecipe>();

        public IReadOnlyList<ItemData> Items => items;
        public IReadOnlyList<ForgingRecipe> Recipes => recipes;

        public void ReplaceContents(IEnumerable<ItemData> configuredItems, 
            IEnumerable<ForgingRecipe> configuredRecipes)
        {
            items = configuredItems != null ? new List<ItemData>(configuredItems) : 
                new List<ItemData>();
            recipes = configuredRecipes != null ? 
                new List<ForgingRecipe>(configuredRecipes) : new List<ForgingRecipe>();
            RemoveNulls();
        }

        public void RemoveNulls()
        {
            items.RemoveAll(item => item == null);
            recipes.RemoveAll(recipe => recipe == null);
        }
    }
}
