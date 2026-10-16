using UnityEngine;

namespace InventoryDemo
{
    /// Composition root. It creates the runtime MVC objects once and exposes them to views.
    public sealed class InventoryDemoBootstrap : MonoBehaviour
    {
        [SerializeField] private InventoryCatalog catalog;
        [SerializeField, Min(1)] private int backpackCapacity;
        [SerializeField, Min(1)] private int refreshCapacity;
        [SerializeField, Min(1)] private int forgeInputCapacity;
        [SerializeField, Min(1)] private int forgeOutputCapacity;

        public InventorySession Session { get; private set; }
        public InventoryController Controller { get; private set; }
        public RefreshController RefreshController { get; private set; }
        public CraftingController CraftingController { get; private set; }
        public InventoryInteractionController Interaction { get; private set; }
        public InventoryCatalog Catalog => catalog;

        private void Awake()
        {
            if (catalog == null)
            {
                catalog = Resources.Load<InventoryCatalog>("Inventory Catalog");
            }

            Session = new InventorySession(backpackCapacity, refreshCapacity, forgeInputCapacity, forgeOutputCapacity);
            Controller = new InventoryController(Session);
            RefreshController = new RefreshController(Session, catalog, Controller);
            CraftingController = new CraftingController(Session, catalog, Controller);
            Interaction = new InventoryInteractionController(Controller);

            if (catalog == null || catalog.Items.Count == 0)
            {
                Controller.Notify("Inventory Catalog is empty. Add ItemData assets to continue.");
                return;
            }

            RefreshController.Refresh();
        }
    }
}
