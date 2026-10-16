using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public sealed class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea] private string description;
    [SerializeField, Min(1)] private int maxStackSize = 99;
    [SerializeField, Header("Spawn Settings"), Range(1, 100)] private int spawnWeight = 10;

    public string ItemId => string.IsNullOrWhiteSpace(itemId) ? name : itemId;
    public string ItemName => string.IsNullOrWhiteSpace(itemName) ? name : itemName;
    public Sprite Icon => icon;
    public string Description => description;
    public int MaxStackSize => Mathf.Max(1, maxStackSize);
    public int SpawnWeight => Mathf.Clamp(spawnWeight, 1, 100);
    public bool IsStackable => MaxStackSize > 1;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemId)) itemId = name;
        if (string.IsNullOrWhiteSpace(itemName)) itemName = name;
        maxStackSize = Mathf.Max(1, maxStackSize);
        spawnWeight = Mathf.Clamp(spawnWeight, 1, 100);
    }
}
