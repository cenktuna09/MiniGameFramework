using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.Events;
using MiniGameFramework.Core.DI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace MiniGameFramework.Core.Input
{
    /// <summary>
    /// Setup script for Input System test scene.
    /// Automatically configures test environment and creates necessary UI components.
    /// </summary>
    public class InputSystemTestSetup : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject canvasPrefab;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private GameObject textPrefab;
        [SerializeField] private GameObject scrollbarPrefab;

        [Header("Setup Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createEventSystem = true;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestEnvironment();
            }
        }

        /// <summary>
        /// Sets up the complete test environment for Input System testing.
        /// </summary>
        [ContextMenu("Setup Test Environment")]
        public void SetupTestEnvironment()
        {
            Debug.Log("InputSystemTestSetup: Setting up test environment...");

            SetupServices();
            SetupUIComponents();
            SetupInputTester();

            Debug.Log("InputSystemTestSetup: Test environment setup complete!");
        }

        private void SetupServices()
        {
            // ServiceLocator is singleton, no need to create GameObject
            
            // Setup InputSystemBootstrapper - it will handle EventBus creation internally
            var bootstrapper = FindObjectOfType<InputSystemBootstrapper>();
            if (bootstrapper == null)
            {
                var bootstrapperGO = new GameObject("InputSystemBootstrapper");
                bootstrapper = bootstrapperGO.AddComponent<InputSystemBootstrapper>();
                DontDestroyOnLoad(bootstrapperGO);
                
                // Force initialization to ensure services are ready
                bootstrapper.InitializeInputSystem();
            }
        }

        private void SetupUIComponents()
        {
            // Create Camera if it doesn't exist
            var camera = FindObjectOfType<Camera>();
            if (camera == null)
            {
                var cameraGO = new GameObject("Main Camera");
                camera = cameraGO.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 0, -10);
            }

            // Create Canvas if it doesn't exist
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("TestCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
                
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Create EventSystem if needed
            if (createEventSystem && FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Setup main test UI structure
            SetupTestUI(canvas.transform);
        }

        private void SetupTestUI(Transform canvasTransform)
        {
            // Main panel
            var mainPanel = CreateUIPanel(canvasTransform, "InputTestPanel", new Vector2(0, 0), new Vector2(1, 1));
            var panelImage = mainPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Title
            var title = CreateTextElement(mainPanel.transform, "Title", "Input System Tester", 24);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Status panel (left side)
            var statusPanel = CreateUIPanel(mainPanel.transform, "StatusPanel", new Vector2(0, 0), new Vector2(0.4f, 0.9f));
            var statusImage = statusPanel.AddComponent<Image>();
            statusImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var statusText = CreateTextElement(statusPanel.transform, "StatusText", "Status: Initializing...", 14);
            ConfigureTextForPanel(statusText, statusPanel.transform);

            // Log panel (right side)
            var logPanel = CreateUIPanel(mainPanel.transform, "LogPanel", new Vector2(0.4f, 0), new Vector2(1, 0.9f));
            var logImage = logPanel.AddComponent<Image>();
            logImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            // Log scroll view
            var scrollView = CreateScrollView(logPanel.transform, "LogScrollView");
            var logText = CreateTextElement(scrollView.transform.Find("Viewport/Content"), "LogText", "Logs will appear here...", 12);
            ConfigureLogText(logText);

            // Button panel
            var buttonPanel = CreateUIPanel(mainPanel.transform, "ButtonPanel", new Vector2(0, 0), new Vector2(0.4f, 0.3f));
            SetupButtonGrid(buttonPanel.transform);
        }

        private GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent, false);
            
            var rectTransform = panelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return panelGO;
        }

        private GameObject CreateTextElement(Transform parent, string name, string text, int fontSize)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);

            var rectTransform = textGO.AddComponent<RectTransform>();
            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.fontSize = fontSize;
                textComponent.color = Color.white;
                textComponent.alignment = TextAlignmentOptions.TopLeft;
                textComponent.enableWordWrapping = true;
            }
            else
            {
                Debug.LogError($"Failed to create TextMeshProUGUI component for {name}");
            }

            return textGO;
        }

        private GameObject CreateScrollView(Transform parent, string name)
        {
            var scrollViewGO = new GameObject(name);
            scrollViewGO.transform.SetParent(parent, false);

            var scrollRect = scrollViewGO.AddComponent<ScrollRect>();
            var scrollRectTransform = scrollViewGO.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;

            // Viewport
            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollViewGO.transform, false);
            var viewportRect = viewportGO.AddComponent<RectTransform>();
            var viewportMask = viewportGO.AddComponent<Mask>();
            var viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = Color.clear;

            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // Content
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            return scrollViewGO;
        }

        private void ConfigureTextForPanel(GameObject textGO, Transform panel)
        {
            var rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);

            var text = textGO.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.alignment = TextAlignmentOptions.TopLeft;
                text.enableWordWrapping = true;
            }
        }

        private void ConfigureLogText(GameObject textGO)
        {
            var rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(0, 1000); // Will grow as needed

            var text = textGO.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.alignment = TextAlignmentOptions.TopLeft;
                text.enableWordWrapping = true;
            }
        }

        private void SetupButtonGrid(Transform buttonPanel)
        {
            var gridLayout = buttonPanel.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(120, 30);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;

            var contentSizeFitter = buttonPanel.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void SetupInputTester()
        {
            var tester = FindObjectOfType<InputSystemTester>();
            if (tester == null)
            {
                var testerGO = new GameObject("InputSystemTester");
                tester = testerGO.AddComponent<InputSystemTester>();
            }

            // Find and assign UI references
            var statusText = GameObject.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var logText = GameObject.Find("LogText")?.GetComponent<TextMeshProUGUI>();
            var buttonPanel = GameObject.Find("ButtonPanel")?.transform;

            if (statusText != null && logText != null && buttonPanel != null)
            {
                // Use reflection to set private fields for testing
                var testerType = typeof(InputSystemTester);
                
                var statusField = testerType.GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                statusField?.SetValue(tester, statusText);
                
                var logField = testerType.GetField("logText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                logField?.SetValue(tester, logText);
                
                var buttonPanelField = testerType.GetField("testUIParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                buttonPanelField?.SetValue(tester, buttonPanel);
            }
        }

        #region Editor Menu Items

#if UNITY_EDITOR
        [MenuItem("MiniGameFramework/Setup/Create Input System Test Scene")]
        public static void CreateInputSystemTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "InputSystemTest";

            // Create setup object
            var setupGO = new GameObject("InputSystemTestSetup");
            var setup = setupGO.AddComponent<InputSystemTestSetup>();
            
            // Trigger setup
            setup.SetupTestEnvironment();

            // Save scene
            var scenePath = "Assets/Scenes/Core/InputSystemTest.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"Input System Test Scene created at: {scenePath}");
        }
#endif

        #endregion
    }
}