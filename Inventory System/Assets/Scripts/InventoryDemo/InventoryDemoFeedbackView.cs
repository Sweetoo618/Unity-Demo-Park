using UnityEngine;
using UnityEngine.UI;

namespace InventoryDemo
{
    /// Presents application feedback without making controllers depend on a concrete UI.
    public sealed class InventoryDemoFeedbackView : MonoBehaviour
    {
        [SerializeField] private InventoryDemoBootstrap bootstrap;
        [SerializeField] private Text label;
        private bool subscribed;

        public void Configure(InventoryDemoBootstrap owner, Text target)
        {
            bootstrap = owner;
            label = target;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            if (subscribed && bootstrap != null && bootstrap.Controller != null)
            {
                bootstrap.Controller.Feedback -= Show;
                subscribed = false;
            }
        }

        private void Subscribe()
        {
            if (!subscribed && bootstrap != null && bootstrap.Controller != null)
            {
                bootstrap.Controller.Feedback += Show;
                subscribed = true;
            }
        }

        private void Show(string message)
        {
            if (label != null)
            {
                label.text = message;
            }
        }
    }
}
