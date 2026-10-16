using UnityEngine;

namespace InventoryDemo
{
    /// Scene-facing command adapter. UnityEvent invokes these methods without exposing
    /// runtime controllers directly to button views.
    public sealed class InventoryDemoUICommands : MonoBehaviour
    {
        [SerializeField] private InventoryDemoBootstrap bootstrap;

        public void RefreshItems()
        {
            if (bootstrap == null || bootstrap.RefreshController == null)
            {
                return;
            }

            bootstrap.RefreshController.Refresh();
        }

        public void Craft()
        {
            if (bootstrap == null || bootstrap.CraftingController == null)
            {
                return;
            }

            bootstrap.CraftingController.TryCraft();
        }

        public void Configure(InventoryDemoBootstrap owner)
        {
            bootstrap = owner;
        }
    }
}
