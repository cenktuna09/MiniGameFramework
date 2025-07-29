using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Loading panel with progress indicators and status messages.
    /// Provides smooth animations and flexible loading state management.
    /// </summary>
    public class LoadingPanel : UIPanel
    {
        [Header("Loading UI Elements")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Image _progressFill;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _spinnerContainer;
        [SerializeField] private Image _spinnerImage;

        [Header("Loading Configuration")]
        [SerializeField] private bool _showProgressBar = true;
        [SerializeField] private bool _showProgressText = true;
        [SerializeField] private bool _showCancelButton = false;
        [SerializeField] private bool _animateProgress = true;
        [SerializeField] private float _progressAnimationSpeed = 2f;
        [SerializeField] private bool _showSpinner = true;
        [SerializeField] private float _spinnerRotationSpeed = 360f;
        [SerializeField] private AnimationCurve _progressEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Loading Messages")]
        [SerializeField] private string[] _loadingMessages = {
            "Loading...",
            "Please wait...",
            "Preparing content...",
            "Almost ready..."
        };
        [SerializeField] private float _messageChangeInterval = 2f;

        // State tracking
        private float _currentProgress = 0f;
        private float _targetProgress = 0f;
        private bool _isLoading = false;
        private Coroutine _progressAnimationCoroutine;
        private Coroutine _messageRotationCoroutine;
        private Coroutine _spinnerCoroutine;
        private int _currentMessageIndex = 0;

        // Progress animation coroutines
        private Coroutine _progressTweenCoroutine;

        #region Events

        /// <summary>
        /// Called when the cancel button is pressed
        /// </summary>
        public event Action OnCancelRequested;

        /// <summary>
        /// Called when loading progress changes
        /// </summary>
        public event Action<float> OnProgressChanged;

        /// <summary>
        /// Called when loading is completed
        /// </summary>
        public event Action OnLoadingCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Current loading progress (0-1)
        /// </summary>
        public float Progress => _currentProgress;

        /// <summary>
        /// Whether loading is currently in progress
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Whether the cancel button is visible and enabled
        /// </summary>
        public bool CanCancel
        {
            get => _cancelButton != null && _cancelButton.gameObject.activeInHierarchy && _cancelButton.interactable;
            set
            {
                if (_cancelButton != null)
                {
                    _cancelButton.gameObject.SetActive(value);
                    _cancelButton.interactable = value;
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        protected override void OnPanelInitialized()
        {
            base.OnPanelInitialized();
            
            SetupUI();
            SetupButtonListeners();
        }

        protected override void OnPanelDestroyed()
        {
            base.OnPanelDestroyed();
            
            StopAllAnimations();
            RemoveButtonListeners();
        }

        #endregion

        #region UI Setup

        private void SetupUI()
        {
            // Configure progress bar
            if (_progressBar != null)
            {
                _progressBar.gameObject.SetActive(_showProgressBar);
                _progressBar.value = 0f;
                _progressBar.minValue = 0f;
                _progressBar.maxValue = 1f;
            }

            // Configure progress text
            if (_progressText != null)
            {
                _progressText.gameObject.SetActive(_showProgressText);
                UpdateProgressText(0f);
            }

            // Configure cancel button
            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(_showCancelButton);
            }

            // Configure spinner
            if (_spinnerContainer != null)
            {
                _spinnerContainer.SetActive(_showSpinner);
            }

            // Set initial loading message
            if (_loadingText != null && _loadingMessages.Length > 0)
            {
                _loadingText.text = _loadingMessages[0];
            }
        }

        private void SetupButtonListeners()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
        }

        private void RemoveButtonListeners()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Starts the loading process
        /// </summary>
        /// <param name="showCancelButton">Whether to show the cancel button</param>
        public void StartLoading(bool showCancelButton = false)
        {
            _isLoading = true;
            _currentProgress = 0f;
            _targetProgress = 0f;
            
            CanCancel = showCancelButton;
            
            UpdateProgress(0f);
            
            if (_showSpinner)
            {
                StartSpinnerAnimation();
            }
            
            if (_loadingMessages.Length > 1)
            {
                StartMessageRotation();
            }
            
            LogIfEnabled("Loading started");
        }

        /// <summary>
        /// Updates the loading progress
        /// </summary>
        /// <param name="progress">Progress value between 0 and 1</param>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            
            if (_animateProgress)
            {
                AnimateProgressTo(progress);
            }
            else
            {
                UpdateProgress(progress);
            }
        }

        /// <summary>
        /// Sets the loading message text
        /// </summary>
        /// <param name="message">Message to display</param>
        public void SetLoadingMessage(string message)
        {
            if (_loadingText != null)
            {
                _loadingText.text = message;
            }
        }

        /// <summary>
        /// Completes the loading process
        /// </summary>
        public void CompleteLoading()
        {
            _isLoading = false;
            
            StopAllAnimations();
            
            if (_animateProgress)
            {
                AnimateProgressTo(1f, () => {
                    OnLoadingCompleted?.Invoke();
                    LogIfEnabled("Loading completed");
                });
            }
            else
            {
                UpdateProgress(1f);
                OnLoadingCompleted?.Invoke();
                LogIfEnabled("Loading completed");
            }
        }

        /// <summary>
        /// Cancels the loading process
        /// </summary>
        public void CancelLoading()
        {
            _isLoading = false;
            StopAllAnimations();
            
            LogIfEnabled("Loading cancelled");
        }

        /// <summary>
        /// Sets the progress bar color
        /// </summary>
        /// <param name="color">Color for the progress fill</param>
        public void SetProgressColor(Color color)
        {
            if (_progressFill != null)
            {
                _progressFill.color = color;
            }
        }

        #endregion

        #region Progress Animation

        private void UpdateProgress(float progress)
        {
            _currentProgress = progress;
            _targetProgress = progress;
            
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }
            
            if (_showProgressText)
            {
                UpdateProgressText(progress);
            }
            
            OnProgressChanged?.Invoke(progress);
        }

        private void UpdateProgressText(float progress)
        {
            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        private void AnimateProgressTo(float targetProgress, Action onComplete = null)
        {
            _targetProgress = targetProgress;
            
            // Cancel existing progress animation
            if (_progressTweenCoroutine != null)
            {
                StopCoroutine(_progressTweenCoroutine);
                _progressTweenCoroutine = null;
            }
            
            // Animate progress with coroutine
            float duration = Mathf.Abs(targetProgress - _currentProgress) / _progressAnimationSpeed;
            duration = Mathf.Max(duration, 0.1f); // Minimum duration
            
            _progressTweenCoroutine = StartCoroutine(AnimateProgressValue(_currentProgress, targetProgress, duration, onComplete));
        }

        private IEnumerator AnimateProgressValue(float fromValue, float toValue, float duration, Action onComplete)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = elapsedTime / duration;
                float easedTime = _progressEaseCurve.Evaluate(normalizedTime);
                
                float currentValue = Mathf.Lerp(fromValue, toValue, easedTime);
                UpdateProgress(currentValue);
                
                yield return null;
            }
            
            UpdateProgress(toValue);
            _progressTweenCoroutine = null;
            onComplete?.Invoke();
        }

        #endregion

        #region Animation Methods

        private void StartSpinnerAnimation()
        {
            if (_spinnerCoroutine != null)
            {
                StopCoroutine(_spinnerCoroutine);
            }
            
            _spinnerCoroutine = StartCoroutine(SpinnerAnimation());
        }

        private void StartMessageRotation()
        {
            if (_messageRotationCoroutine != null)
            {
                StopCoroutine(_messageRotationCoroutine);
            }
            
            _messageRotationCoroutine = StartCoroutine(MessageRotationAnimation());
        }

        private IEnumerator SpinnerAnimation()
        {
            while (_isLoading && _spinnerImage != null)
            {
                _spinnerImage.transform.Rotate(0, 0, -_spinnerRotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator MessageRotationAnimation()
        {
            while (_isLoading && _loadingMessages.Length > 1)
            {
                yield return new WaitForSeconds(_messageChangeInterval);
                
                if (_isLoading)
                {
                    _currentMessageIndex = (_currentMessageIndex + 1) % _loadingMessages.Length;
                    
                    if (_loadingText != null)
                    {
                        _loadingText.text = _loadingMessages[_currentMessageIndex];
                    }
                }
            }
        }

        private void StopAllAnimations()
        {
            // Stop progress animation
            if (_progressTweenCoroutine != null)
            {
                StopCoroutine(_progressTweenCoroutine);
                _progressTweenCoroutine = null;
            }
            
            // Stop coroutines
            if (_spinnerCoroutine != null)
            {
                StopCoroutine(_spinnerCoroutine);
                _spinnerCoroutine = null;
            }
            
            if (_messageRotationCoroutine != null)
            {
                StopCoroutine(_messageRotationCoroutine);
                _messageRotationCoroutine = null;
            }
        }

        #endregion

        #region Event Handlers

        private void OnCancelButtonClicked()
        {
            LogIfEnabled("Cancel button clicked");
            OnCancelRequested?.Invoke();
        }

        #endregion

        #region Panel Lifecycle Overrides

        protected override void OnPanelShowStarted()
        {
            base.OnPanelShowStarted();
            LogIfEnabled("Loading panel is showing");
        }

        protected override void OnPanelShowCompleted()
        {
            base.OnPanelShowCompleted();
            LogIfEnabled("Loading panel is now visible");
        }

        protected override void OnPanelHideStarted()
        {
            base.OnPanelHideStarted();
            LogIfEnabled("Loading panel is hiding");
            
            // Stop all animations when hiding
            StopAllAnimations();
        }

        protected override void OnPanelHideCompleted()
        {
            base.OnPanelHideCompleted();
            LogIfEnabled("Loading panel is now hidden");
            
            // Reset state when hidden
            _isLoading = false;
            _currentProgress = 0f;
            _targetProgress = 0f;
            _currentMessageIndex = 0;
        }

        #endregion

        #region Private Methods

        private void LogIfEnabled(string message)
        {
            Debug.Log($"[LoadingPanel] {message}", this);
        }

        #endregion
    }
} 