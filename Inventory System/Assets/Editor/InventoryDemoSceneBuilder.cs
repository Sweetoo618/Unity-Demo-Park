using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InventoryDemo.Editor
{
    /// Builds the complete UGUI demo scene from Unity built-in controls.
    public static class InventoryDemoSceneBuilder
    {
        private const string Root = "Assets/InventoryDemo";
        private const string ScenePath = Root + "/InventoryDemo.unity";
        private const string PrefabPath = Root + "/Prefabs/InventorySlot.prefab";

        [MenuItem("Inventory Demo/Create or Rebuild MVC Demo Scene")]
        public static void Build()
        {
            EnsureFolder(Root);
            EnsureFolder(Root + "/Prefabs");
            EnsureFolder(Root + "/Resources");

            InventoryCatalog catalog = AssetDatabase.LoadAssetAtPath<InventoryCatalog>(Root + "/Resources/Inventory Catalog.asset");
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<InventoryCatalog>();
                AssetDatabase.CreateAsset(catalog, Root + "/Resources/Inventory Catalog.asset");
            }

            catalog.ReplaceContents(LoadItems("Assets/SO/Items"), LoadRecipes("Assets/SO/ForgingRecipes"));
            EditorUtility.SetDirty(catalog);
            CreateSlotPrefab();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            CreateEventSystem();
            GameObject canvasObject = CreateCanvas();
            InventoryDemoBootstrap bootstrap = CreateBootstrap(catalog);
            GameObject root = CreatePanel(canvasObject.transform, "Inventory Workbench", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(1160, 650), new Color(0.08f, 0.1f, 0.14f, 0.98f));
            InventoryDemoUICommands commands = bootstrap.gameObject.AddComponent<InventoryDemoUICommands>();
            commands.Configure(bootstrap);

            CreateText(root.transform, "INVENTORY WORKBENCH", new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -26), new Vector2(1000, 42), 28, TextAnchor.MiddleCenter);
            CreateText(root.transform, "Refresh -> Backpack -> Forge Input -> Craft -> Forge Output -> Backpack",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -62), new Vector2(1000, 28), 15,
                TextAnchor.MiddleCenter);

            Text feedback = CreateText(root.transform, "Ready", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 58), new Vector2(1080, 30), 16, TextAnchor.MiddleCenter);
            InventoryDemoFeedbackView feedbackView = feedback.gameObject.AddComponent<InventoryDemoFeedbackView>();
            feedbackView.Configure(bootstrap, feedback);
            CreateArea(root.transform, bootstrap, "REFRESH ITEMS", InventoryArea.Refresh, new Vector2(0.25f, 0.65f),
                new Vector2(245, 220), new Vector2(0, 0), feedback);
            CreateArea(root.transform, bootstrap, "BACKPACK", InventoryArea.Backpack, new Vector2(0.75f, 0.65f),
                new Vector2(520, 220), new Vector2(0, 0), feedback);
            CreateArea(root.transform, bootstrap, "FORGE INPUT", InventoryArea.ForgeInput, new Vector2(0.25f, 0.25f),
                new Vector2(245, 150), new Vector2(0, 0), feedback);
            CreateArea(root.transform, bootstrap, "FORGE OUTPUT", InventoryArea.ForgeOutput, new Vector2(0.75f, 0.25f),
                new Vector2(245, 150), new Vector2(0, 0), feedback);

            Button refreshButton = CreateButton(root.transform, "Refresh Items", new Vector2(-120, 0), new Vector2(190, 36),
                new Color(0.18f, 0.42f, 0.68f, 1));
            UnityEventTools.AddPersistentListener(refreshButton.onClick, commands.RefreshItems);
            Button craftButton = CreateButton(root.transform, "Craft", new Vector2(120, 0), new Vector2(190, 36),
                new Color(0.2f, 0.55f, 0.34f, 1));
            UnityEventTools.AddPersistentListener(craftButton.onClick, commands.Craft);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Inventory MVC demo scene rebuilt at " + ScenePath);
        }

        private static InventoryDemoBootstrap CreateBootstrap(InventoryCatalog catalog)
        {
            GameObject go = new GameObject("Inventory MVC Bootstrap");
            InventoryDemoBootstrap bootstrap = go.AddComponent<InventoryDemoBootstrap>();
            SetPrivate(bootstrap, "catalog", catalog);
            return bootstrap;
        }

        private static void CreateArea(Transform parent, InventoryDemoBootstrap bootstrap, string label,
            InventoryArea area, Vector2 anchor, Vector2 size, Vector2 offset, Text feedback)
        {
            GameObject panel = CreatePanel(parent, label, anchor, new Vector2(0.5f, 0.5f), size,
                new Color(0.06f, 0.07f, 0.1f, 0.96f));
            panel.GetComponent<RectTransform>().anchoredPosition = offset;
            CreateText(panel.transform, label, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18),
                new Vector2(size.x - 20, 28), 17, TextAnchor.MiddleCenter);

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            content.transform.SetParent(panel.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0);
            contentRect.anchorMax = new Vector2(0.5f, 0);
            contentRect.pivot = new Vector2(0.5f, 0);
            contentRect.anchoredPosition = new Vector2(0, 12);
            contentRect.sizeDelta = new Vector2(size.x - 20, size.y - 52);
            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(58, 58);
            grid.spacing = new Vector2(6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = area == InventoryArea.Backpack ? 6 : 3;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(4, 4, 4, 4);

            InventoryAreaView view = panel.AddComponent<InventoryAreaView>();
            SetPrivate(view, "bootstrap", bootstrap);
            SetPrivate(view, "area", area);
            SetPrivate(view, "content", contentRect);
            SetPrivate(view, "slotPrefab", AssetDatabase.LoadAssetAtPath<InventorySlotView>(PrefabPath));
        }

        private static void CreateSlotPrefab()
        {
            GameObject slot = new GameObject("Inventory Slot", typeof(RectTransform), typeof(Image), typeof(InventorySlotView));
            Image background = slot.GetComponent<Image>();
            background.color = new Color(0.12f, 0.14f, 0.16f, 1);
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(slot.transform, false);
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            Image icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            GameObject amountObject = new GameObject("Amount", typeof(RectTransform), typeof(Text));
            amountObject.transform.SetParent(slot.transform, false);
            RectTransform amountRect = amountObject.GetComponent<RectTransform>();
            amountRect.anchorMin = Vector2.zero;
            amountRect.anchorMax = Vector2.one;
            amountRect.offsetMin = new Vector2(3, 2);
            amountRect.offsetMax = new Vector2(-4, -2);
            Text amount = amountObject.GetComponent<Text>();
            amount.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            amount.fontSize = 16;
            amount.alignment = TextAnchor.LowerRight;
            amount.color = Color.white;
            amount.raycastTarget = false;
            InventorySlotView slotView = slot.GetComponent<InventorySlotView>();
            SetPrivate(slotView, "icon", icon);
            SetPrivate(slotView, "amount", amount);
            SetPrivate(slotView, "background", background);
            PrefabUtility.SaveAsPrefabAsset(slot, PrefabPath);
            Object.DestroyImmediate(slot);
        }

        private static GameObject CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Inventory Demo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            return canvasObject;
        }

        private static void CreateEventSystem() => new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Text CreateText(Transform parent, string value, Vector2 anchor, Vector2 pivot, Vector2 position,
            Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(value, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 offset, Vector2 size, Color color)
        {
            GameObject go = CreatePanel(parent, label + " Button", new Vector2(0.5f, 0), new Vector2(0.5f, 0), size, color);
            go.GetComponent<RectTransform>().anchoredPosition = offset;
            Button button = go.AddComponent<Button>();
            CreateText(go.transform, label, Vector2.zero, Vector2.zero, Vector2.zero, size, 15, TextAnchor.MiddleCenter);
            return button;
        }

        private static List<ItemData> LoadItems(string folder)
        {
            List<ItemData> result = new List<ItemData>();
            foreach (string guid in AssetDatabase.FindAssets("t:ItemData", new[] { folder }))
            {
                result.Add(AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return result;
        }

        private static List<ForgingRecipe> LoadRecipes(string folder)
        {
            List<ForgingRecipe> result = new List<ForgingRecipe>();
            foreach (string guid in AssetDatabase.FindAssets("t:ForgingRecipe", new[] { folder }))
            {
                result.Add(AssetDatabase.LoadAssetAtPath<ForgingRecipe>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return result;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }
    }
}
