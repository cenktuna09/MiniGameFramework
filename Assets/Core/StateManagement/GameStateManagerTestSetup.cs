#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MiniGameFramework.Core.StateManagement
{
    /// <summary>
    /// Editor utility to automatically set up the GameStateManagerTest scene.
    /// Creates all necessary UI components and configures the tester.
    /// </summary>
    public static class GameStateManagerTestSetup
    {
        [MenuItem("MiniGameFramework/Setup/Create GameStateManager Test Scene")]
        public static void CreateTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "GameStateManagerTest";

            // Create UI Canvas
            var canvasGO = CreateCanvas();
            var canvas = canvasGO.GetComponent<Canvas>();

            // Create GameBootstrap
            var bootstrapGO = CreateGameBootstrap();

            // Create UI Layout
            CreateUILayout(canvas);

            // Create and configure tester
            var testerGO = CreateGameStateManagerTester();

            // Save scene
            string scenePath = "Assets/Scenes/Core/GameStateManagerTest.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[GameStateManagerTestSetup] Test scene created successfully at: {scenePath}");
        }

        private static GameObject CreateCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            return canvasGO;
        }

        private static GameObject CreateGameBootstrap()
        {
            var bootstrapGO = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGO.AddComponent<MiniGameFramework.Core.Bootstrap.GameBootstrap>();
            
            return bootstrapGO;
        }

        private static void CreateUILayout(Canvas canvas)
        {
            var canvasTransform = canvas.transform;

            // Create main panel
            var mainPanel = CreatePanel("MainPanel", canvasTransform);
            var mainLayout = mainPanel.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 10;
            mainLayout.padding = new RectOffset(20, 20, 20, 20);
            mainLayout.childControlHeight = false;
            mainLayout.childControlWidth = true;

            // Title
            CreateText("Game State Manager Tester", mainPanel.transform, 24, Color.white);

            // Status display
            CreateText("Status: Ready", mainPanel.transform, 16, Color.green, "StatusText");

            // State info section
            var stateInfoPanel = CreatePanel("StateInfoPanel", mainPanel.transform);
            var stateInfoLayout = stateInfoPanel.AddComponent<HorizontalLayoutGroup>();
            stateInfoLayout.spacing = 20;
            stateInfoLayout.childControlWidth = true;

            // Current state display
            CreateText("Current: Menu\nPrevious: Menu", stateInfoPanel.transform, 14, Color.cyan, "CurrentStateText");

            // Valid transitions display
            CreateText("Valid Transitions:\nLoading", stateInfoPanel.transform, 14, Color.yellow, "ValidTransitionsText");

            // State history display
            CreateText("History:\nMenu", stateInfoPanel.transform, 14, Color.magenta, "StateHistoryText");

            // Manual controls section
            var controlsPanel = CreatePanel("ControlsPanel", mainPanel.transform);
            var controlsLayout = controlsPanel.AddComponent<HorizontalLayoutGroup>();
            controlsLayout.spacing = 10;
            controlsLayout.childControlWidth = true;

            CreateButton("Menu", controlsPanel.transform, "MenuButton");
            CreateButton("Loading", controlsPanel.transform, "LoadingButton");
            CreateButton("Playing", controlsPanel.transform, "PlayingButton");
            CreateButton("Paused", controlsPanel.transform, "PausedButton");
            CreateButton("GameOver", controlsPanel.transform, "GameOverButton");

            // Test controls section
            var testPanel = CreatePanel("TestPanel", mainPanel.transform);
            var testLayout = testPanel.AddComponent<VerticalLayoutGroup>();
            testLayout.spacing = 10;

            var testRow1 = CreatePanel("TestRow1", testPanel.transform);
            var testRow1Layout = testRow1.AddComponent<HorizontalLayoutGroup>();
            testRow1Layout.spacing = 10;
            testRow1Layout.childControlWidth = true;

            CreateButton("Test Valid Transitions", testRow1.transform, "TestValidTransitionsButton");
            CreateButton("Test Invalid Transitions", testRow1.transform, "TestInvalidTransitionsButton");
            CreateButton("Test Custom Rules", testRow1.transform, "TestCustomRulesButton");

            var testRow2 = CreatePanel("TestRow2", testPanel.transform);
            var testRow2Layout = testRow2.AddComponent<HorizontalLayoutGroup>();
            testRow2Layout.spacing = 10;
            testRow2Layout.childControlWidth = true;

            CreateButton("Test Event System", testRow2.transform, "TestEventSystemButton");
            CreateButton("Test All", testRow2.transform, "TestAllButton");
            CreateButton("Reset State", testRow2.transform, "ResetStateButton");
        }

        private static GameObject CreateGameStateManagerTester()
        {
            var testerGO = new GameObject("GameStateManagerTester");
            var tester = testerGO.AddComponent<GameStateManagerTester>();

            // Find and assign UI references
            var statusText = GameObject.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            var currentStateText = GameObject.Find("CurrentStateText")?.GetComponent<TextMeshProUGUI>();
            var validTransitionsText = GameObject.Find("ValidTransitionsText")?.GetComponent<TextMeshProUGUI>();
            var stateHistoryText = GameObject.Find("StateHistoryText")?.GetComponent<TextMeshProUGUI>();

            var menuButton = GameObject.Find("MenuButton")?.GetComponent<Button>();
            var loadingButton = GameObject.Find("LoadingButton")?.GetComponent<Button>();
            var playingButton = GameObject.Find("PlayingButton")?.GetComponent<Button>();
            var pausedButton = GameObject.Find("PausedButton")?.GetComponent<Button>();
            var gameOverButton = GameObject.Find("GameOverButton")?.GetComponent<Button>();

            var testValidButton = GameObject.Find("TestValidTransitionsButton")?.GetComponent<Button>();
            var testInvalidButton = GameObject.Find("TestInvalidTransitionsButton")?.GetComponent<Button>();
            var testCustomButton = GameObject.Find("TestCustomRulesButton")?.GetComponent<Button>();
            var testEventButton = GameObject.Find("TestEventSystemButton")?.GetComponent<Button>();
            var testAllButton = GameObject.Find("TestAllButton")?.GetComponent<Button>();
            var resetButton = GameObject.Find("ResetStateButton")?.GetComponent<Button>();

            // Use reflection to set private fields
            var testerType = typeof(GameStateManagerTester);
            SetPrivateField(tester, "_statusText", statusText);
            SetPrivateField(tester, "_currentStateText", currentStateText);
            SetPrivateField(tester, "_validTransitionsText", validTransitionsText);
            SetPrivateField(tester, "_stateHistoryText", stateHistoryText);

            SetPrivateField(tester, "_menuButton", menuButton);
            SetPrivateField(tester, "_loadingButton", loadingButton);
            SetPrivateField(tester, "_playingButton", playingButton);
            SetPrivateField(tester, "_pausedButton", pausedButton);
            SetPrivateField(tester, "_gameOverButton", gameOverButton);

            SetPrivateField(tester, "_testValidTransitionsButton", testValidButton);
            SetPrivateField(tester, "_testInvalidTransitionsButton", testInvalidButton);
            SetPrivateField(tester, "_testCustomRulesButton", testCustomButton);
            SetPrivateField(tester, "_testEventSystemButton", testEventButton);
            SetPrivateField(tester, "_testAllButton", testAllButton);
            SetPrivateField(tester, "_resetStateButton", resetButton);

            return testerGO;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static GameObject CreatePanel(string name, Transform parent)
        {
            var panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent);

            var rectTransform = panelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            return panelGO;
        }

        private static GameObject CreateButton(string text, Transform parent, string name = null)
        {
            var buttonGO = new GameObject(name ?? text + "Button");
            buttonGO.transform.SetParent(parent);

            var rectTransform = buttonGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 30);

            var image = buttonGO.AddComponent<Image>();
            image.color = Color.white;

            var button = buttonGO.AddComponent<Button>();

            // Create text child
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textMesh = textGO.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.black;
            textMesh.fontSize = 12;

            button.targetGraphic = image;

            return buttonGO;
        }

        private static GameObject CreateText(string text, Transform parent, int fontSize = 14, Color? color = null, string name = null)
        {
            var textGO = new GameObject(name ?? "Text");
            textGO.transform.SetParent(parent);

            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);

            var textMesh = textGO.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color ?? Color.white;
            textMesh.alignment = TextAlignmentOptions.TopLeft;

            return textGO;
        }
    }
}
#endif 