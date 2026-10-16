using UnityEngine;
using UnityEngine.EventSystems;

namespace InventoryDemo
{
    /// Reusable UGUI view for one runtime inventory area.
    public sealed class InventoryAreaView : MonoBehaviour, IDropHandler
    {
        [SerializeField] private InventoryDemoBootstrap bootstrap;
        [SerializeField] private InventoryArea area;
        [SerializeField] private RectTransform content;
        [SerializeField] private InventorySlotView slotPrefab;
        private InventorySlotView[] slots;

        public InventoryArea Area => area;

        public void Configure(InventoryDemoBootstrap owner, InventoryArea targetArea,
            RectTransform targetContent, InventorySlotView prefab)
        {
            bootstrap = owner;
            area = targetArea;
            content = targetContent;
            slotPrefab = prefab;
        }

        private void Start()
        {
            if (bootstrap == null || content == null || slotPrefab == null)
            {
                Debug.LogError("InventoryAreaView is missing its bootstrap, content or slot prefab.", this);
                return;
            }

            InventoryModel model = bootstrap.Session.Get(area);
            model.Changed += Render;
            slots = new InventorySlotView[model.Capacity];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = Instantiate(slotPrefab, content);
                slots[i].Bind(this, i);
            }

            Render();
        }

        private void OnDestroy()
        {
            if (bootstrap != null && bootstrap.Session != null)
            {
                bootstrap.Session.Get(area).Changed -= Render;
            }
        }

        public void Render()
        {
            if (bootstrap == null || slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Render(bootstrap.Controller.Get(area, i));
            }
        }

        public void BeginDrag(int index) => bootstrap?.Interaction.BeginDrag(area, index);

        public void Drop(int index) => bootstrap?.Interaction.DropOn(area, index);

        public void Discard(int index)
        {
            bootstrap?.Controller.Drop(area, index);
        }

        public void Split(int index)
        {
            if (bootstrap == null || bootstrap.Controller == null)
            {
                return;
            }

            ItemSlot slot = bootstrap.Controller.Get(area, index);
            if (slot == null || slot.IsEmpty || slot.stackCount < 2)
            {
                return;
            }

            int splitAmount = Mathf.Max(1, slot.stackCount / 2);
            if (bootstrap.Controller.Split(area, index, splitAmount))
            {
                bootstrap.Controller.Notify("Split " + splitAmount + " " + slot.itemData.ItemName + ".");
            }
        }

        public void OnDrop(PointerEventData eventData) => bootstrap?.Interaction.DropOnArea(area);
    }
}
