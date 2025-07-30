#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using MiniGameFramework.Core.Bootstrap;
using System.Reflection;
using System;

namespace MiniGameFramework.Core.GameManagement.Editor
{
    /// <summary>
    /// Editor utility to automatically create and configure GameManager test scene.
    /// Creates complete UI layout and sets up all necessary components for testing.
    /// </summary>
    public static class GameManagerTestSetup
    {
        [MenuItem("MiniGameFramework/Setup/Create GameManager Test Scene")]
        public static void CreateGameManagerTestScene()
        {
            // Create new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "GameManagerTest";

            Debug.Log("[GameManagerTestSetup] Creating GameManager test scene...");

            try
            {
                // Create scene hierarchy
                CreateSceneHierarchy();
                Debug.Log("[GameManagerTestSetup] ✅ GameManager test scene created successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameManagerTestSetup] ❌ Failed to create test scene: {ex.Message}");
            }
        }

        private static void CreateSceneHierarchy()
        {
            // Create Camera
            CreateCamera();

            // Create EventSystem for UI interaction
            CreateEventSystem();

            // Create Canvas
            var canvas = CreateCanvas();

            // Create GameBootstrap
            CreateGameBootstrap();

            // Create GameManagerTester with UI
            CreateGameManagerTesterWithUI(canvas);

            Debug.Log("[GameManagerTestSetup] Scene hierarchy created successfully");
        }

        private static void CreateCamera()
        {
            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            cameraGO.AddComponent<AudioListener>();
            cameraGO.tag = "MainCamera";

            Debug.Log("[GameManagerTestSetup] Camera created");
        }

        private static void CreateEventSystem()
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            Debug.Log("[GameManagerTestSetup] EventSystem created");
        }

        private static Canvas CreateCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            Debug.Log("[GameManagerTestSetup] Canvas created");
            return canvas;
        }

        private static void CreateGameBootstrap()
        {
            var bootstrapGO = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGO.AddComponent<GameBootstrap>();

            // Configure bootstrap
            var serializedObject = new SerializedObject(bootstrap);
            serializedObject.FindProperty("_initializeOnAwake").boolValue = true;
            serializedObject.FindProperty("_dontDestroyOnLoad").boolValue = false; // Keep in scene for testing
            serializedObject.FindProperty("_logBootstrapProcess").boolValue = true;
            serializedObject.ApplyModifiedProperties();

            Debug.Log("[GameManagerTestSetup] GameBootstrap created and configured");
        }

        private static void CreateGameManagerTesterWithUI(Canvas canvas)
        {
            // Create main tester object
            var testerGO = new GameObject("GameManagerTester");
            var tester = testerGO.AddComponent<GameManagerTester>();

            // Configure tester
            var serializedObject = new SerializedObject(tester);
            serializedObject.FindProperty("autoStartTests").boolValue = true;
            serializedObject.FindProperty("testInterval").floatValue = 2f;
            serializedObject.FindProperty("runContinuousTests").boolValue = false;

            // Create UI Layout
            CreateUILayout(canvas, tester, serializedObject);

            serializedObject.ApplyModifiedProperties();

            Debug.Log("[GameManagerTester] GameManagerTester created with full UI");
        }

        private static void CreateUILayout(Canvas canvas, GameManagerTester tester, SerializedObject serializedObject)
        {
            // Create main panel
            var mainPanel = CreateUIPanel(canvas.transform, "GameManagerTestUI", new Vector2(0, 0), new Vector2(1, 1));
            
            // Create layout groups with correct anchoring
            var topSection = CreateUISection(mainPanel.transform, "TopSection", new Vector2(0, 0.6f), new Vector2(1, 1.0f));
            var middleSection = CreateUISection(mainPanel.transform, "MiddleSection", new Vector2(0, 0.3f), new Vector2(1, 0.6f));
            var bottomSection = CreateUISection(mainPanel.transform, "BottomSection", new Vector2(0, 0), new Vector2(1, 0.3f));

            // Create UI sections
            CreateTopSection(topSection.transform, tester, serializedObject);
            CreateMiddleSection(middleSection.transform, tester, serializedObject);
            CreateBottomSection(bottomSection.transform, tester, serializedObject);
        }

        private static void CreateTopSection(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            var horizontalLayout = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 30;
            horizontalLayout.padding = new RectOffset(15, 15, 15, 15);
            horizontalLayout.childControlWidth = true;
            horizontalLayout.childControlHeight = false;

            // Left side - Game Management Controls
            var leftPanel = CreateUIPanel(parent, "GameControls", Vector2.zero, Vector2.one);
            leftPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 0);

            CreateTitle(leftPanel.transform, "Game Management Controls");
            CreateGameManagementControls(leftPanel.transform, tester, serializedObject);

            // Center - Session and Systems Controls  
            var centerPanel = CreateUIPanel(parent, "SessionControls", Vector2.zero, Vector2.one);
            centerPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 0);

            CreateTitle(centerPanel.transform, "Session & Systems Controls");
            CreateSessionAndSystemControls(centerPanel.transform, tester, serializedObject);

            // Right side - Test Controls
            var rightPanel = CreateUIPanel(parent, "TestControls", Vector2.zero, Vector2.one);
            rightPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 0);

            CreateTitle(rightPanel.transform, "Test Controls");
            CreateTestControls(rightPanel.transform, tester, serializedObject);
        }

        private static void CreateMiddleSection(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            var horizontalLayout = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 20;
            horizontalLayout.padding = new RectOffset(20, 20, 20, 20);

            // Real-time displays
            CreateStatusDisplays(parent, tester, serializedObject);
        }

        private static void CreateBottomSection(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            var verticalLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.padding = new RectOffset(20, 20, 20, 20);

            // Test results display
            CreateTestResultsDisplay(parent, tester, serializedObject);
        }

        private static void CreateGameManagementControls(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            var verticalLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5;

            // Game lifecycle buttons
            var loadButton = CreateButton(parent, "Load Game", Color.green);
            var startButton = CreateButton(parent, "Start Game", Color.cyan);
            var pauseButton = CreateButton(parent, "Pause Game", Color.yellow);
            var resumeButton = CreateButton(parent, "Resume Game", Color.cyan);
            var endButton = CreateButton(parent, "End Game", Color.red);
            var unloadButton = CreateButton(parent, "Unload Game", Color.gray);

            // Assign to serialized properties
            serializedObject.FindProperty("loadGameButton").objectReferenceValue = loadButton;
            serializedObject.FindProperty("startGameButton").objectReferenceValue = startButton;
            serializedObject.FindProperty("pauseGameButton").objectReferenceValue = pauseButton;
            serializedObject.FindProperty("resumeGameButton").objectReferenceValue = resumeButton;
            serializedObject.FindProperty("endGameButton").objectReferenceValue = endButton;
            serializedObject.FindProperty("unloadGameButton").objectReferenceValue = unloadButton;
        }

        private static void CreateSessionAndSystemControls(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            var verticalLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5;

            // Score input and controls
            var scoreInput = CreateInputField(parent, "Score Input", "100");
            var updateScoreButton = CreateButton(parent, "Update Score", Color.blue);
            var addScoreButton = CreateButton(parent, "Add Score", Color.blue);

            // Timer controls
            var startTimerButton = CreateButton(parent, "Start Timer", Color.green);
            var pauseTimerButton = CreateButton(parent, "Pause Timer", Color.yellow);
            var resetTimerButton = CreateButton(parent, "Reset Timer", Color.gray);

            // Progress controls
            var advanceProgressButton = CreateButton(parent, "Advance Progress", Color.magenta);
            var resetProgressButton = CreateButton(parent, "Reset Progress", Color.gray);

            // Assign to serialized properties
            serializedObject.FindProperty("scoreInputField").objectReferenceValue = scoreInput;
            serializedObject.FindProperty("updateScoreButton").objectReferenceValue = updateScoreButton;
            serializedObject.FindProperty("addScoreButton").objectReferenceValue = addScoreButton;
            serializedObject.FindProperty("startTimerButton").objectReferenceValue = startTimerButton;
            serializedObject.FindProperty("pauseTimerButton").objectReferenceValue = pauseTimerButton;
            serializedObject.FindProperty("resetTimerButton").objectReferenceValue = resetTimerButton;
            serializedObject.FindProperty("advanceProgressButton").objectReferenceValue = advanceProgressButton;
            serializedObject.FindProperty("resetProgressButton").objectReferenceValue = resetProgressButton;
        }

        private static void CreateTestControls(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            var verticalLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 5;

            // Test execution buttons
            var runAllButton = CreateButton(parent, "🔄 Run All Tests", new Color(0.2f, 0.8f, 0.2f));
            var bootstrapTestButton = CreateButton(parent, "Test Bootstrap", Color.cyan);
            var lifecycleTestButton = CreateButton(parent, "Test Lifecycle", Color.blue);
            var sessionTestButton = CreateButton(parent, "Test Session", Color.magenta);
            var timerTestButton = CreateButton(parent, "Test Progress/Timer", Color.yellow);

            // Make the Run All Tests button larger and more prominent
            var runAllRect = runAllButton.GetComponent<RectTransform>();
            runAllRect.sizeDelta = new Vector2(runAllRect.sizeDelta.x, 50);

            // Assign to serialized properties
            serializedObject.FindProperty("runAllTestsButton").objectReferenceValue = runAllButton;
            serializedObject.FindProperty("testBootstrapIntegrationButton").objectReferenceValue = bootstrapTestButton;
            serializedObject.FindProperty("testGameLifecycleButton").objectReferenceValue = lifecycleTestButton;
            serializedObject.FindProperty("testSessionTrackingButton").objectReferenceValue = sessionTestButton;
            serializedObject.FindProperty("testProgressTimerButton").objectReferenceValue = timerTestButton;
        }

        private static void CreateStatusDisplays(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            // Create display panels
            var leftDisplay = CreateUIPanel(parent, "LeftDisplays", Vector2.zero, Vector2.one);
            leftDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 0);

            var centerDisplay = CreateUIPanel(parent, "CenterDisplays", Vector2.zero, Vector2.one);
            centerDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 0);

            var rightDisplay = CreateUIPanel(parent, "RightDisplays", Vector2.zero, Vector2.one);
            rightDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 0);

            // Left displays
            var leftLayout = leftDisplay.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 10;

            CreateTitle(leftDisplay.transform, "System Status");
            var statusText = CreateTextDisplay(leftDisplay.transform, "Status: Initializing...", 14);
            var gameManagerStateText = CreateTextDisplay(leftDisplay.transform, "GameManager: Not Available", 12);

            // Center displays  
            var centerLayout = centerDisplay.AddComponent<VerticalLayoutGroup>();
            centerLayout.spacing = 10;

            CreateTitle(centerDisplay.transform, "Game & Session Info");
            var currentGameText = CreateTextDisplay(centerDisplay.transform, "No Game Loaded", 12);
            var sessionInfoText = CreateTextDisplay(centerDisplay.transform, "No Active Session", 12);

            // Right displays
            var rightLayout = rightDisplay.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 10;

            CreateTitle(rightDisplay.transform, "Timer & Progress");
            var timerDisplayText = CreateTextDisplay(rightDisplay.transform, "Timer: 00:00\nRunning: False", 12);
            var progressDisplayText = CreateTextDisplay(rightDisplay.transform, "Progress: 0%\nStep: 0/10", 12);

            // Progress bar
            var progressBar = CreateProgressBar(rightDisplay.transform);

            // Assign to serialized properties
            serializedObject.FindProperty("statusText").objectReferenceValue = statusText;
            serializedObject.FindProperty("gameManagerStateText").objectReferenceValue = gameManagerStateText;
            serializedObject.FindProperty("currentGameText").objectReferenceValue = currentGameText;
            serializedObject.FindProperty("sessionInfoText").objectReferenceValue = sessionInfoText;
            serializedObject.FindProperty("timerDisplayText").objectReferenceValue = timerDisplayText;
            serializedObject.FindProperty("progressDisplayText").objectReferenceValue = progressDisplayText;
            serializedObject.FindProperty("progressBar").objectReferenceValue = progressBar;
        }

        private static void CreateTestResultsDisplay(Transform parent, GameManagerTester tester, SerializedObject serializedObject)
        {
            CreateTitle(parent, "Test Results");
            
            // Create scrollable test results
            var scrollView = CreateScrollView(parent, new Vector2(0, 0), new Vector2(1500, 250)); // Larger scroll area
            var testResultsText = CreateTextDisplay(scrollView.content, "Test results will appear here...", 12); // Larger font
            testResultsText.alignment = TextAlignmentOptions.TopLeft;

            // Assign to serialized property
            serializedObject.FindProperty("testResultsText").objectReferenceValue = testResultsText;
        }

        #region UI Creation Helpers

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var panelGO = new GameObject(name, typeof(RectTransform));
            panelGO.transform.SetParent(parent, false);

            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.1f);

            return panelGO;
        }

        private static GameObject CreateUISection(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var sectionGO = CreateUIPanel(parent, name, anchorMin, anchorMax);
            var image = sectionGO.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            return sectionGO;
        }

        private static Button CreateButton(Transform parent, string text, Color color)
        {
            var buttonGO = new GameObject($"Button_{text.Replace(" ", "")}", typeof(RectTransform));
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 35); // Larger buttons

            var image = buttonGO.AddComponent<Image>();
            image.color = color;

            var button = buttonGO.AddComponent<Button>();

            // Add text
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = Color.white;
            textComponent.fontSize = 12;
            textComponent.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private static TextMeshProUGUI CreateTextDisplay(Transform parent, string text, int fontSize)
        {
            var textGO = new GameObject($"Text_{text.Substring(0, System.Math.Min(10, text.Length))}", typeof(RectTransform));
            textGO.transform.SetParent(parent, false);

            var rect = textGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(480, fontSize * 4); // Much wider text area

            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = Color.white;
            textComponent.fontSize = fontSize + 2; // Slightly larger font
            textComponent.alignment = TextAlignmentOptions.TopLeft;
            textComponent.enableWordWrapping = true; // Enable word wrapping

            return textComponent;
        }

        private static void CreateTitle(Transform parent, string titleText)
        {
            var titleGO = new GameObject($"Title_{titleText.Replace(" ", "")}", typeof(RectTransform));
            titleGO.transform.SetParent(parent, false);

            var rect = titleGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 35); // Fixed width to 250

            var textComponent = titleGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = titleText;
            textComponent.color = Color.yellow;
            textComponent.fontSize = 16;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.alignment = TextAlignmentOptions.Center;
        }

        private static TMP_InputField CreateInputField(Transform parent, string placeholder, string defaultValue)
        {
            var inputGO = new GameObject($"InputField_{placeholder.Replace(" ", "")}", typeof(RectTransform));
            inputGO.transform.SetParent(parent, false);

            var rect = inputGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 35); // Larger input fields

            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var inputField = inputGO.AddComponent<TMP_InputField>();

            // Create text area
            var textAreaGO = new GameObject("Text Area", typeof(RectTransform));
            textAreaGO.transform.SetParent(inputGO.transform, false);

            var textAreaRect = textAreaGO.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.sizeDelta = Vector2.zero;
            textAreaRect.anchoredPosition = Vector2.zero;

            // Create text component
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(textAreaGO.transform, false);

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.color = Color.white;
            textComponent.fontSize = 12;

            inputField.textComponent = textComponent;
            inputField.text = defaultValue;

            return inputField;
        }

        private static Slider CreateProgressBar(Transform parent)
        {
            var sliderGO = new GameObject("ProgressBar", typeof(RectTransform));
            sliderGO.transform.SetParent(parent, false);

            var rect = sliderGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(450, 25); // Wider progress bar

            var slider = sliderGO.AddComponent<Slider>();

            // Background
            var backgroundGO = new GameObject("Background", typeof(RectTransform));
            backgroundGO.transform.SetParent(sliderGO.transform, false);

            var bgRect = backgroundGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = backgroundGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill Area
            var fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGO.transform.SetParent(sliderGO.transform, false);

            var fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            // Fill
            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(fillAreaGO.transform, false);

            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            var fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.green;

            slider.fillRect = fillRect;
            slider.value = 0f;

            return slider;
        }

        private static ScrollRect CreateScrollView(Transform parent, Vector2 position, Vector2 size)
        {
            var scrollViewGO = new GameObject("ScrollView", typeof(RectTransform));
            scrollViewGO.transform.SetParent(parent, false);

            var rect = scrollViewGO.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var scrollRect = scrollViewGO.AddComponent<ScrollRect>();

            // Viewport
            var viewportGO = new GameObject("Viewport", typeof(RectTransform));
            viewportGO.transform.SetParent(scrollViewGO.transform, false);

            var viewportRect = viewportGO.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            var viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var mask = viewportGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewportGO.transform, false);

            var contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(0, 300);
            contentRect.anchoredPosition = Vector2.zero;

            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            return scrollRect;
        }

        #endregion
    }
}
#endif 