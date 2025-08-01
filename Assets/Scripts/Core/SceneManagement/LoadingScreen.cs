using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Architecture;
using MiniGameFramework.Core.SceneManagement;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Loading screen UI component with progress tracking and customizable content.
    /// Displays loading progress and status messages during scene transitions.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Progress Settings")]
        [SerializeField] private bool smoothProgress = true;
        [SerializeField] private float progressSmoothSpeed = 2f;
        [SerializeField] private string defaultLoadingText = "Loading...";
        
        private IEventBus _eventBus;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private float _targetProgress;
        private float _currentProgress;
        private bool _isVisible;
        private bool _isAnimating;
        
        /// <summary>
        /// Indicates if the loading screen is currently visible.
        /// </summary>
        public bool IsVisible => _isVisible;
        
        /// <summary>
        /// Current loading progress (0.0 to 1.0).
        /// </summary>
        public float Progress => _currentProgress;
        
        private void Awake()
        {
            SetupCanvas();
            SetupCanvasGroup();
            InitializeComponents();
            
            // Start hidden (immediate, no await needed)
            _ = SetVisibility(false, immediate: true);
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        private void Update()
        {
            if (smoothProgress && Mathf.Abs(_targetProgress - _currentProgress) > 0.001f)
            {
                _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, progressSmoothSpeed * Time.unscaledDeltaTime);
                UpdateProgressUI();
            }
        }
        
        /// <summary>
        /// Initialize the loading screen with dependency injection.
        /// </summary>
        /// <param name="eventBus">Event bus for listening to scene loading events</param>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
        }
        
        /// <summary>
        /// Show the loading screen with optional custom text.
        /// </summary>
        /// <param name="loadingMessage">Custom loading message (uses default if null)</param>
        /// <returns>Task that completes when the show animation is finished</returns>
        public async Task ShowAsync(string loadingMessage = null)
        {
            if (_isVisible || _isAnimating) return;
            
            SetLoadingText(loadingMessage ?? defaultLoadingText);
            SetProgress(0f);
            
            await SetVisibility(true);
        }
        
        /// <summary>
        /// Hide the loading screen.
        /// </summary>
        /// <returns>Task that completes when the hide animation is finished</returns>
        public async Task HideAsync()
        {
            if (!_isVisible || _isAnimating) return;
            
            await SetVisibility(false);
        }
        
        /// <summary>
        /// Update the loading progress.
        /// </summary>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
            
            if (!smoothProgress)
            {
                _currentProgress = _targetProgress;
                UpdateProgressUI();
            }
        }
        
        /// <summary>
        /// Update the loading text message.
        /// </summary>
        /// <param name="message">Loading message to display</param>
        public void SetLoadingText(string message)
        {
            if (loadingText != null)
            {
                loadingText.text = message ?? defaultLoadingText;
            }
        }
        
        /// <summary>
        /// Update both progress and text simultaneously.
        /// </summary>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        /// <param name="message">Loading message to display</param>
        public void UpdateProgress(float progress, string message = null)
        {
            SetProgress(progress);
            if (!string.IsNullOrEmpty(message))
            {
                SetLoadingText(message);
            }
        }
        

        
        private void SetupCanvas()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }
            
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1001; // Above transitions so loading screen is visible
            
            if (GetComponent<CanvasScaler>() == null)
            {
                var canvasScaler = gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
            
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // LoadingScreen should persist across scene changes
            DontDestroyOnLoad(gameObject);
        }
        
        private void SetupCanvasGroup()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        private void InitializeComponents()
        {
            // Create loading panel if not assigned
            if (loadingPanel == null)
            {
                CreateDefaultLoadingPanel();
            }
            
            // Initialize progress bar
            if (progressBar != null)
            {
                progressBar.minValue = 0f;
                progressBar.maxValue = 1f;
                progressBar.value = 0f;
            }
            
            // Set default loading text
            if (loadingText != null)
            {
                loadingText.text = defaultLoadingText;
            }
        }
        
        private void CreateDefaultLoadingPanel()
        {
            // This creates a basic loading panel if none is assigned
            // In a real project, you'd typically design this in the Unity editor
            
            var panelGO = new GameObject("LoadingPanel");
            panelGO.transform.SetParent(transform, false);
            
            var rectTransform = panelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent black
            
            loadingPanel = panelGO;
            backgroundImage = image;
            
            // Add progress bar and text (basic implementations)
            CreateDefaultProgressBar();
            CreateDefaultLoadingText();
            
            // Progress text will be created in Start method to avoid order issues
        }
        
        private void CreateDefaultProgressBar()
        {
            var progressGO = new GameObject("ProgressBar");
            progressGO.transform.SetParent(loadingPanel.transform, false);
            
            var rectTransform = progressGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.2f, 0.4f);
            rectTransform.anchorMax = new Vector2(0.8f, 0.5f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            progressBar = progressGO.AddComponent<Slider>();
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
            
            // Basic slider background and fill
            var background = new GameObject("Background");
            background.transform.SetParent(progressGO.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(progressGO.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;
            
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.white;
            
            progressBar.targetGraphic = fillImage;
            progressBar.fillRect = fillRect;
        }
        
        private void CreateDefaultLoadingText()
        {
            var textGO = new GameObject("LoadingText");
            textGO.transform.SetParent(loadingPanel.transform, false);
            
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.2f, 0.55f);
            rectTransform.anchorMax = new Vector2(0.8f, 0.65f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
                        // Use TMPro for loading text
            loadingText = textGO.AddComponent<TextMeshProUGUI>();
            loadingText.text = defaultLoadingText;
            loadingText.fontSize = 24f;
            loadingText.color = Color.white;
            loadingText.alignment = TextAlignmentOptions.Center;
        }
        private async Task SetVisibility(bool visible, bool immediate = false)
        {
            if (_isAnimating) return;
            
            _isVisible = visible;
            
            if (immediate)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = visible ? 1f : 0f;
                    _canvasGroup.blocksRaycasts = visible;
                }
                loadingPanel?.SetActive(visible);
                return;
            }
            
            _isAnimating = true;
            loadingPanel?.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = visible;
            }
            
            try
            {
                var duration = visible ? fadeInDuration : fadeOutDuration;
                var startAlpha = _canvasGroup.alpha;
                var targetAlpha = visible ? 1f : 0f;
                var elapsedTime = 0f;
                
                while (elapsedTime < duration && _canvasGroup != null)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    var progress = elapsedTime / duration;
                    var curveValue = fadeCurve.Evaluate(progress);
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                    
                    await Task.Yield();
                }
                
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = targetAlpha;
                }
                
                if (!visible)
                {
                    loadingPanel?.SetActive(false);
                }
            }
            finally
            {
                _isAnimating = false;
            }
        }
        
        private void UpdateProgressUI()
        {
            if (progressBar != null)
            {
                progressBar.value = _currentProgress;
            }
            
            if (progressText != null)
            {
                var percentage = Mathf.RoundToInt(_currentProgress * 100);
                progressText.text = $"{percentage}%";
            }
        }
        
        private void SubscribeToEvents()
        {
            if (_eventBus == null) 
            {
                Debug.LogError("[LoadingScreen] EventBus is null! Cannot subscribe to events.");
                return;
            }
            
            _eventBus.Subscribe<SceneLoadingStartedEvent>(OnSceneLoadingStarted);
            _eventBus.Subscribe<SceneLoadingCompletedEvent>(OnSceneLoadingCompleted);
            _eventBus.Subscribe<SceneLoadingProgressEvent>(OnSceneLoadingProgress);
        }
        
        private async void OnSceneLoadingStarted(SceneLoadingStartedEvent eventData)
        {
            await ShowAsync($"Loading {eventData.SceneName}...");
        }
        
        private async void OnSceneLoadingCompleted(SceneLoadingCompletedEvent eventData)
        {
            SetProgress(1f);
            await Task.Delay(200); // Brief delay to show 100%
            await HideAsync();
        }
        
        private void OnSceneLoadingProgress(SceneLoadingProgressEvent eventData)
        {
            UpdateProgress(eventData.Progress, eventData.LoadingText);
        }
        
        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<SceneLoadingStartedEvent>(OnSceneLoadingStarted);
                _eventBus.Unsubscribe<SceneLoadingCompletedEvent>(OnSceneLoadingCompleted);
                _eventBus.Unsubscribe<SceneLoadingProgressEvent>(OnSceneLoadingProgress);
            }
        }
    }
} 