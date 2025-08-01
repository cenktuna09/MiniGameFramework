using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Core.Architecture;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Implements fade transition using a UI Image overlay.
    /// Provides smooth fade-in and fade-out effects for scene transitions.
    /// </summary>
    public class FadeTransition : MonoBehaviour, ISceneTransition
    {
        [Header("Fade Settings")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private Color fadeColor = Color.black;
        
        private IEventBus _eventBus;
        private Canvas _fadeCanvas;
        private bool _isTransitioning;
        private SceneTransitionData _transitionData;
        
        /// <inheritdoc />
        public TransitionType TransitionType => TransitionType.Fade;
        
        /// <inheritdoc />
        public float Duration => _transitionData.transitionDuration > 0 ? _transitionData.transitionDuration : defaultDuration;
        
        /// <inheritdoc />
        public bool IsTransitioning => _isTransitioning;
        
        private void Awake()
        {
            SetupFadeCanvas();
            SetupFadeImage();
            
            // Start with transparent - ensure visible gameplay
            SetAlpha(0f);
        }
        
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        /// <inheritdoc />
        public void Initialize(SceneTransitionData transitionData)
        {
            _transitionData = transitionData;
            
            if (fadeImage != null)
            {
                fadeImage.color = transitionData.fadeColor;
            }
            
            fadeCurve = transitionData.transitionCurve ?? fadeCurve;
            fadeColor = transitionData.fadeColor;
        }
        
        /// <inheritdoc />
        public async Task FadeOutAsync(float? duration = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Fade transition already in progress");
                return;
            }
            
            _isTransitioning = true;
            var fadeDuration = duration ?? Duration;
            
            try
            {
                PublishTransitionEvent(TransitionState.FadeOutStarted);
                
                await FadeToAlpha(1f, fadeDuration);
                
                PublishTransitionEvent(TransitionState.FadeOutCompleted);
            }
            finally
            {
                _isTransitioning = false;
            }
        }
        
        /// <inheritdoc />
        public async Task FadeInAsync(float? duration = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Fade transition already in progress");
                return;
            }
            
            _isTransitioning = true;
            var fadeDuration = duration ?? Duration;
            
            try
            {
                PublishTransitionEvent(TransitionState.FadeInStarted);
                
                await FadeToAlpha(0f, fadeDuration);
                
                PublishTransitionEvent(TransitionState.FadeInCompleted);
            }
            finally
            {
                _isTransitioning = false;
            }
        }
        
        /// <inheritdoc />
        public async Task TransitionAsync(float? duration = null)
        {
            await FadeOutAsync(duration);
            await FadeInAsync(duration);
        }
        
        /// <inheritdoc />
        public void Reset()
        {
            _isTransitioning = false;
            SetAlpha(0f);
        }
        
        private void Start()
        {
            // Ensure we start transparent after all initialization
            SetAlpha(0f);
        }
        
        private void SetupFadeCanvas()
        {
            if (_fadeCanvas == null)
            {
                var canvasGO = new GameObject("FadeCanvas");
                canvasGO.transform.SetParent(transform);
                
                _fadeCanvas = canvasGO.AddComponent<Canvas>();
                _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _fadeCanvas.sortingOrder = 1000; // High priority to render on top
                
                var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasGO.AddComponent<GraphicRaycaster>();
                
                DontDestroyOnLoad(canvasGO);
            }
        }
        
        private void SetupFadeImage()
        {
            if (fadeImage == null)
            {
                var imageGO = new GameObject("FadeImage");
                imageGO.transform.SetParent(_fadeCanvas.transform, false);
                
                fadeImage = imageGO.AddComponent<Image>();
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Start transparent!
                
                // Fill the entire screen
                var rectTransform = fadeImage.rectTransform;
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        
        private async Task FadeToAlpha(float targetAlpha, float duration)
        {
            if (fadeImage == null)
            {
                Debug.LogError("Fade image is not set up properly");
                return;
            }
            
            var startAlpha = fadeImage.color.a;
            var elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                var progress = elapsedTime / duration;
                var curveValue = fadeCurve.Evaluate(progress);
                var currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                
                SetAlpha(currentAlpha);
                
                await Task.Yield();
            }
            
            SetAlpha(targetAlpha);
        }
        
        private void SetAlpha(float alpha)
        {
            if (fadeImage != null)
            {
                var color = fadeImage.color;
                color.a = alpha;
                fadeImage.color = color;
            }
        }
        
        private void PublishTransitionEvent(TransitionState state)
        {
            _eventBus?.Publish(new SceneTransitionEvent(state, "", TransitionType.Fade, this));
        }
        
        private void OnDestroy()
        {
            if (_fadeCanvas != null && _fadeCanvas.gameObject != null)
            {
                Destroy(_fadeCanvas.gameObject);
            }
        }
    }
} 