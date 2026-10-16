using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventoryDemo
{
    /// UGUI slot: renders a model snapshot and forwards pointer intent to its area view.
    public sealed class InventorySlotView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IDropHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text amount;
        [SerializeField] private Image background;
        private InventoryAreaView areaOwner;
        private int index;

        public void Bind(InventoryAreaView panel, int slotIndex)
        {
            areaOwner = panel;
            index = slotIndex;
        }

        public void Render(ItemSlot slot)
        {
            bool hasItem = slot != null && !slot.IsEmpty;
            if (icon != null)
            {
                icon.enabled = hasItem && slot.itemData.Icon != null;
                icon.sprite = hasItem ? slot.itemData.Icon : null;
                icon.color = hasItem && slot.itemData.Icon == null ? ColorFor(slot.itemData) : Color.white;
            }
            if (amount != null)
            {
                amount.enabled = hasItem && slot.stackCount > 1;
                amount.text = hasItem && slot.stackCount > 1 ? slot.stackCount.ToString() : string.Empty;
            }
            if (background != null) 
                background.color = hasItem ? new Color(0.18f, 0.2f, 0.23f, 1f) : 
                    new Color(0.12f, 0.14f, 0.16f, 1f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Middle)
            {
                areaOwner?.Split(index);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Handle right-click on press so a right-button drag cannot cancel the click event.
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                areaOwner?.Discard(index);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && areaOwner != null)
            {
                areaOwner.BeginDrag(index);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // The event-system marker is required for Unity to keep dispatching drag events.
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            if (areaOwner != null) areaOwner.Drop(index);
        }

        private static Color ColorFor(ItemData item)
        {
            string key = (item.ItemName + " " + item.ItemId).ToLowerInvariant();
            if (key.Contains("red")) return new Color(0.9f, 0.25f, 0.22f);
            if (key.Contains("green")) return new Color(0.28f, 0.72f, 0.4f);
            if (key.Contains("purple")) return new Color(0.62f, 0.38f, 0.8f);
            if (key.Contains("yellow")) return new Color(0.92f, 0.72f, 0.24f);
            if (key.Contains("mask")) return new Color(0.72f, 0.74f, 0.76f);
            if (key.Contains("knife")) return new Color(0.55f, 0.68f, 0.76f);
            if (key.Contains("watch")) return new Color(0.88f, 0.66f, 0.25f);
            if (key.Contains("biscuit")) return new Color(0.73f, 0.52f, 0.31f);
            if (key.Contains("match")) return new Color(0.9f, 0.42f, 0.22f);
            return new Color(0.62f, 0.68f, 0.7f);
        }
    }
}
