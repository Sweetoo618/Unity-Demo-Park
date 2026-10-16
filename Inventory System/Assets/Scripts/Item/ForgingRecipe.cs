using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Forging Recipe", menuName = "Inventory/Forging Recipe", order = 51)]
public sealed class ForgingRecipe : ScriptableObject
{
    [SerializeField] private string recipeName = "New Recipe";
    [SerializeField] private List<ItemRequirement> inputItems = new List<ItemRequirement>();
    [SerializeField] private ItemData outputItem;
    [SerializeField, Min(1)] private int outputCount = 1;

    public string RecipeName => string.IsNullOrWhiteSpace(recipeName) ? name : recipeName;
    public IReadOnlyList<ItemRequirement> InputItems => inputItems;
    public ItemData OutputItem => outputItem;
    public int OutputCount => Mathf.Max(1, outputCount);

    public bool Matches(ItemSlot[] slots)
    {
        if (slots == null || inputItems == null || inputItems.Count == 0 || outputItem == null)
        {
            return false;
        }

        foreach (ItemRequirement requirement in inputItems)
        {
            if (requirement == null || !requirement.IsMet(slots))
            {
                return false;
            }
        }

        return true;
    }

    private void OnValidate()
    {
        outputCount = Mathf.Max(1, outputCount);
        if (inputItems == null)
        {
            inputItems = new List<ItemRequirement>();
        }
    }
}

[System.Serializable]
public sealed class ItemRequirement
{
    [SerializeField] private ItemData itemData;
    [SerializeField, Min(1)] private int count = 1;

    public ItemData ItemData => itemData;
    public int Count => Mathf.Max(1, count);

    public bool IsMet(ItemSlot[] slots)
    {
        if (itemData == null || slots == null)
        {
            return false;
        }

        int foundCount = 0;
        foreach (ItemSlot slot in slots)
        {
            if (slot == null || slot.IsEmpty || 
                !InventoryDemo.InventoryModel.AreSameItem(slot.itemData, itemData))
            {
                continue;
            }

            foundCount += slot.stackCount;
            if (foundCount >= Count)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        count = Mathf.Max(1, count);
    }
}
