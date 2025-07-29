using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Base class for all UI panels with lifecycle management and Unity default animations.
    /// Provides consistent behavior for panel transitions, state management, and event handling.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        /// <summary>
        /// Represents the current state of the UI panel
        /// </summary>
        public enum PanelState
        {
            Hidden,
            Showing,
            Visible,
            Hiding
        }

        /// <summary>
        /// Defines different transition animation types for panels
        /// </summary>
        public enum TransitionType
        {
            Fade,
            Scale,
            SlideFromTop,
            SlideFromBottom,
            SlideFromLeft,
            SlideFromRight
        }

        [Header("Panel Configuration")]
        [SerializeField] private TransitionType _showTransition = TransitionType.Fade;
        [SerializeField] private TransitionType _hideTransition = TransitionType.Fade;
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool _blockRaycastsWhenHidden = true;

        // Cached components
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        
        // State management
        private PanelState _currentState = PanelState.Hidden;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        
        // Animation tracking
        private Coroutine _currentAnimation;

        #region Properties

        /// <summary>
        /// Current state of the panel
        /// </summary>
        public PanelState CurrentState => _currentState;

        /// <summary>
        /// Whether the panel is currently visible (Visible or Showing states)
        /// </summary>
        public bool IsVisible => _currentState == PanelState.Visible || _currentState == PanelState.Showing;

        /// <summary>
        /// Whether the panel is currently animating (Showing or Hiding states)
        /// </summary>
        public bool IsAnimating => _currentState == PanelState.Showing || _currentState == PanelState.Hiding;

        /// <summary>
        /// CanvasGroup component for controlling panel opacity and interactivity
        /// </summary>
        protected CanvasGroup CanvasGroup => _canvasGroup;

        /// <summary>
        /// RectTransform component for position and scale animations
        /// </summary>
        protected RectTransform RectTransform => _rectTransform;

        #endregion

        #region Events

        /// <summary>
        /// Called when the panel starts showing (before animation)
        /// </summary>
        public event Action<UIPanel> OnShowStarted;

        /// <summary>
        /// Called when the panel finishes showing (after animation)
        /// </summary>
        public event Action<UIPanel> OnShowCompleted;

        /// <summary>
        /// Called when the panel starts hiding (before animation)
        /// </summary>
        public event Action<UIPanel> OnHideStarted;

        /// <summary>
        /// Called when the panel finishes hiding (after animation)
        /// </summary>
        public event Action<UIPanel> OnHideCompleted;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeComponents();
            CacheOriginalValues();
            InitializeHiddenState();
        }

        protected virtual void Start()
        {
            OnPanelInitialized();
        }

        protected virtual void OnDestroy()
        {
            CancelCurrentAnimation();
            OnPanelDestroyed();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_canvasGroup == null)
            {
                Debug.LogError($"UIPanel '{name}' requires a CanvasGroup component!", this);
            }
        }

        private void CacheOriginalValues()
        {
            _originalScale = _rectTransform.localScale;
            _originalPosition = _rectTransform.anchoredPosition;
        }

        private void InitializeHiddenState()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = !_blockRaycastsWhenHidden;
            gameObject.SetActive(false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the panel with animation
        /// </summary>
        /// <returns>Task that completes when the show animation finishes</returns>
        public async Task ShowAsync()
        {
            if (_currentState == PanelState.Visible || _currentState == PanelState.Showing)
                return;

            CancelCurrentAnimation();
            
            _currentState = PanelState.Showing;
            gameObject.SetActive(true);
            
            OnShowStarted?.Invoke(this);
            OnPanelShowStarted();

            await PlayShowAnimation();
            
            _currentState = PanelState.Visible;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            OnShowCompleted?.Invoke(this);
            OnPanelShowCompleted();
        }

        /// <summary>
        /// Hides the panel with animation
        /// </summary>
        /// <returns>Task that completes when the hide animation finishes</returns>
        public async Task HideAsync()
        {
            if (_currentState == PanelState.Hidden || _currentState == PanelState.Hiding)
                return;

            CancelCurrentAnimation();
            
            _currentState = PanelState.Hiding;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = _blockRaycastsWhenHidden;
            
            OnHideStarted?.Invoke(this);
            OnPanelHideStarted();

            await PlayHideAnimation();
            
            _currentState = PanelState.Hidden;
            gameObject.SetActive(false);
            
            OnHideCompleted?.Invoke(this);
            OnPanelHideCompleted();
        }

        /// <summary>
        /// Shows the panel immediately without animation
        /// </summary>
        public void ShowImmediate()
        {
            CancelCurrentAnimation();
            
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.localScale = _originalScale;
            _rectTransform.anchoredPosition = _originalPosition;
            
            _currentState = PanelState.Visible;
            
            OnShowStarted?.Invoke(this);
            OnPanelShowStarted();
            OnShowCompleted?.Invoke(this);
            OnPanelShowCompleted();
        }

        /// <summary>
        /// Hides the panel immediately without animation
        /// </summary>
        public void HideImmediate()
        {
            CancelCurrentAnimation();
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = !_blockRaycastsWhenHidden;
            gameObject.SetActive(false);
            
            _currentState = PanelState.Hidden;
            
            OnHideStarted?.Invoke(this);
            OnPanelHideStarted();
            OnHideCompleted?.Invoke(this);
            OnPanelHideCompleted();
        }

        #endregion

        #region Animation Methods

        private async Task PlayShowAnimation()
        {
            PrepareShowAnimation();
            
            var tcs = new TaskCompletionSource<bool>();
            
            switch (_showTransition)
            {
                case TransitionType.Fade:
                    _currentAnimation = StartCoroutine(AnimateCanvasAlpha(0f, 1f, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.Scale:
                    _rectTransform.localScale = Vector3.zero;
                    _canvasGroup.alpha = 1f;
                    _currentAnimation = StartCoroutine(AnimateScale(Vector3.zero, _originalScale, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromTop:
                    var topPosition = _originalPosition + Vector3.up * Screen.height;
                    _rectTransform.anchoredPosition = topPosition;
                    _canvasGroup.alpha = 1f;
                    _currentAnimation = StartCoroutine(AnimatePosition(topPosition, _originalPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromBottom:
                    var bottomPosition = _originalPosition + Vector3.down * Screen.height;
                    _rectTransform.anchoredPosition = bottomPosition;
                    _canvasGroup.alpha = 1f;
                    _currentAnimation = StartCoroutine(AnimatePosition(bottomPosition, _originalPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromLeft:
                    var leftPosition = _originalPosition + Vector3.left * Screen.width;
                    _rectTransform.anchoredPosition = leftPosition;
                    _canvasGroup.alpha = 1f;
                    _currentAnimation = StartCoroutine(AnimatePosition(leftPosition, _originalPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromRight:
                    var rightPosition = _originalPosition + Vector3.right * Screen.width;
                    _rectTransform.anchoredPosition = rightPosition;
                    _canvasGroup.alpha = 1f;
                    _currentAnimation = StartCoroutine(AnimatePosition(rightPosition, _originalPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
            }
            
            await tcs.Task;
            _currentAnimation = null;
        }

        private async Task PlayHideAnimation()
        {
            var tcs = new TaskCompletionSource<bool>();
            
            switch (_hideTransition)
            {
                case TransitionType.Fade:
                    _currentAnimation = StartCoroutine(AnimateCanvasAlpha(1f, 0f, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.Scale:
                    _currentAnimation = StartCoroutine(AnimateScale(_originalScale, Vector3.zero, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromTop:
                    var topPosition = _originalPosition + Vector3.up * Screen.height;
                    _currentAnimation = StartCoroutine(AnimatePosition(_originalPosition, topPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromBottom:
                    var bottomPosition = _originalPosition + Vector3.down * Screen.height;
                    _currentAnimation = StartCoroutine(AnimatePosition(_originalPosition, bottomPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromLeft:
                    var leftPosition = _originalPosition + Vector3.left * Screen.width;
                    _currentAnimation = StartCoroutine(AnimatePosition(_originalPosition, leftPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
                    
                case TransitionType.SlideFromRight:
                    var rightPosition = _originalPosition + Vector3.right * Screen.width;
                    _currentAnimation = StartCoroutine(AnimatePosition(_originalPosition, rightPosition, _animationDuration, () => tcs.SetResult(true)));
                    break;
            }
            
            await tcs.Task;
            _currentAnimation = null;
        }

        private IEnumerator AnimateCanvasAlpha(float fromAlpha, float toAlpha, float duration, System.Action onComplete)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = elapsedTime / duration;
                float easedTime = _easeCurve.Evaluate(normalizedTime);
                
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, easedTime);
                
                yield return null;
            }
            
            _canvasGroup.alpha = toAlpha;
            onComplete?.Invoke();
        }

        private IEnumerator AnimateScale(Vector3 fromScale, Vector3 toScale, float duration, System.Action onComplete)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = elapsedTime / duration;
                float easedTime = _easeCurve.Evaluate(normalizedTime);
                
                _rectTransform.localScale = Vector3.Lerp(fromScale, toScale, easedTime);
                
                yield return null;
            }
            
            _rectTransform.localScale = toScale;
            onComplete?.Invoke();
        }

        private IEnumerator AnimatePosition(Vector3 fromPosition, Vector3 toPosition, float duration, System.Action onComplete)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = elapsedTime / duration;
                float easedTime = _easeCurve.Evaluate(normalizedTime);
                
                _rectTransform.anchoredPosition = Vector3.Lerp(fromPosition, toPosition, easedTime);
                
                yield return null;
            }
            
            _rectTransform.anchoredPosition = toPosition;
            onComplete?.Invoke();
        }

        private void PrepareShowAnimation()
        {
            switch (_showTransition)
            {
                case TransitionType.Fade:
                    _canvasGroup.alpha = 0f;
                    _rectTransform.localScale = _originalScale;
                    _rectTransform.anchoredPosition = _originalPosition;
                    break;
                    
                case TransitionType.Scale:
                    _rectTransform.localScale = Vector3.zero;
                    _rectTransform.anchoredPosition = _originalPosition;
                    break;
                    
                // Position-based transitions are handled in PlayShowAnimation
            }
        }

        private void CancelCurrentAnimation()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }
        }

        #endregion

        #region Virtual Methods (Override in derived classes)

        /// <summary>
        /// Called during Awake after components are initialized
        /// </summary>
        protected virtual void OnPanelInitialized() { }

        /// <summary>
        /// Called when the panel starts showing (before animation)
        /// </summary>
        protected virtual void OnPanelShowStarted() { }

        /// <summary>
        /// Called when the panel finishes showing (after animation)
        /// </summary>
        protected virtual void OnPanelShowCompleted() { }

        /// <summary>
        /// Called when the panel starts hiding (before animation)
        /// </summary>
        protected virtual void OnPanelHideStarted() { }

        /// <summary>
        /// Called when the panel finishes hiding (after animation)
        /// </summary>
        protected virtual void OnPanelHideCompleted() { }

        /// <summary>
        /// Called during OnDestroy for cleanup
        /// </summary>
        protected virtual void OnPanelDestroyed() { }

        #endregion
    }
} 