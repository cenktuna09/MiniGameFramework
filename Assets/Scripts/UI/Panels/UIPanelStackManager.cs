using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Manages a stack of UI panels for navigation and lifecycle management.
    /// Provides consistent navigation behavior, back stack management, and event handling.
    /// </summary>
    public class UIPanelStackManager : MonoBehaviour
    {
        /// <summary>
        /// Navigation result for panel operations
        /// </summary>
        public enum NavigationResult
        {
            Success,
            PanelNotFound,
            StackEmpty,
            OperationInProgress,
            PanelAlreadyActive
        }

        [Header("Stack Configuration")]
        [SerializeField] private bool _allowDuplicatesInStack = false;
        [SerializeField] private int _maxStackSize = 20;
        [SerializeField] private bool _hideAllOnStart = true;
        [SerializeField] private bool _logNavigationEvents = false;

        // Panel management
        private readonly Stack<UIPanel> _panelStack = new();
        private readonly Dictionary<string, UIPanel> _registeredPanels = new();
        private readonly HashSet<UIPanel> _activePanels = new();
        
        // State tracking
        private bool _isNavigating = false;
        private UIPanel _currentPanel;

        #region Properties

        /// <summary>
        /// Currently active panel at the top of the stack
        /// </summary>
        public UIPanel CurrentPanel => _currentPanel;

        /// <summary>
        /// Number of panels currently in the stack
        /// </summary>
        public int StackCount => _panelStack.Count;

        /// <summary>
        /// Whether the stack manager is currently processing a navigation operation
        /// </summary>
        public bool IsNavigating => _isNavigating;

        /// <summary>
        /// Whether there are panels to navigate back to
        /// </summary>
        public bool CanGoBack => _panelStack.Count > 1;

        /// <summary>
        /// All panels currently registered with this stack manager
        /// </summary>
        public IReadOnlyDictionary<string, UIPanel> RegisteredPanels => _registeredPanels;

        #endregion

        #region Events

        /// <summary>
        /// Called when a panel is pushed onto the stack
        /// </summary>
        public event Action<UIPanel> OnPanelPushed;

        /// <summary>
        /// Called when a panel is popped from the stack
        /// </summary>
        public event Action<UIPanel> OnPanelPopped;

        /// <summary>
        /// Called when the current panel changes
        /// </summary>
        public event Action<UIPanel, UIPanel> OnCurrentPanelChanged; // (previous, current)

        /// <summary>
        /// Called when the stack becomes empty
        /// </summary>
        public event Action OnStackEmpty;

        /// <summary>
        /// Called when navigation starts
        /// </summary>
        public event Action<UIPanel> OnNavigationStarted;

        /// <summary>
        /// Called when navigation completes
        /// </summary>
        public event Action<UIPanel> OnNavigationCompleted;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_hideAllOnStart)
            {
                AutoRegisterPanels();
                HideAllPanelsImmediate();
            }
        }

        private void Start()
        {
            LogIfEnabled("UIPanelStackManager initialized");
        }

        private void Update()
        {
            HandleBackButtonInput();
        }

        #endregion

        #region Panel Registration

        /// <summary>
        /// Automatically registers all UIPanel components found as children
        /// </summary>
        public void AutoRegisterPanels()
        {
            var panels = GetComponentsInChildren<UIPanel>(true);
            
            foreach (var panel in panels)
            {
                RegisterPanel(panel.name, panel);
            }
            
            LogIfEnabled($"Auto-registered {panels.Length} panels");
        }

        /// <summary>
        /// Registers a panel with the stack manager
        /// </summary>
        /// <param name="panelId">Unique identifier for the panel</param>
        /// <param name="panel">Panel instance to register</param>
        public void RegisterPanel(string panelId, UIPanel panel)
        {
            if (string.IsNullOrEmpty(panelId))
            {
                Debug.LogError("Panel ID cannot be null or empty!", this);
                return;
            }

            if (panel == null)
            {
                Debug.LogError($"Cannot register null panel with ID '{panelId}'!", this);
                return;
            }

            if (_registeredPanels.ContainsKey(panelId))
            {
                Debug.LogWarning($"Panel with ID '{panelId}' is already registered. Replacing with new instance.", this);
            }

            _registeredPanels[panelId] = panel;
            
            // Subscribe to panel events
            panel.OnShowCompleted += OnPanelShowCompleted;
            panel.OnHideCompleted += OnPanelHideCompleted;
            
            LogIfEnabled($"Registered panel: {panelId}");
        }

        /// <summary>
        /// Unregisters a panel from the stack manager
        /// </summary>
        /// <param name="panelId">ID of the panel to unregister</param>
        public bool UnregisterPanel(string panelId)
        {
            if (_registeredPanels.TryGetValue(panelId, out var panel))
            {
                // Unsubscribe from panel events
                panel.OnShowCompleted -= OnPanelShowCompleted;
                panel.OnHideCompleted -= OnPanelHideCompleted;
                
                // Remove from stack if present
                RemoveFromStack(panel);
                
                _registeredPanels.Remove(panelId);
                _activePanels.Remove(panel);
                
                LogIfEnabled($"Unregistered panel: {panelId}");
                return true;
            }

            return false;
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Pushes a panel onto the stack and shows it
        /// </summary>
        /// <param name="panelId">ID of the panel to push</param>
        /// <param name="hideCurrentPanel">Whether to hide the current panel</param>
        /// <returns>Navigation result</returns>
        public async Task<NavigationResult> PushPanelAsync(string panelId, bool hideCurrentPanel = true)
        {
            if (_isNavigating)
                return NavigationResult.OperationInProgress;

            if (!_registeredPanels.TryGetValue(panelId, out var panel))
                return NavigationResult.PanelNotFound;

            if (!_allowDuplicatesInStack && _panelStack.Contains(panel))
                return NavigationResult.PanelAlreadyActive;

            _isNavigating = true;
            OnNavigationStarted?.Invoke(panel);

            try
            {
                // Hide current panel if requested
                if (hideCurrentPanel && _currentPanel != null)
                {
                    await _currentPanel.HideAsync();
                }

                // Add to stack and show
                _panelStack.Push(panel);
                var previousPanel = _currentPanel;
                _currentPanel = panel;
                _activePanels.Add(panel);

                await panel.ShowAsync();

                // Fire events
                OnPanelPushed?.Invoke(panel);
                OnCurrentPanelChanged?.Invoke(previousPanel, _currentPanel);
                OnNavigationCompleted?.Invoke(panel);

                LogIfEnabled($"Pushed panel: {panelId}");
                return NavigationResult.Success;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Pops the current panel from the stack and shows the previous one
        /// </summary>
        /// <returns>Navigation result</returns>
        public async Task<NavigationResult> PopPanelAsync()
        {
            if (_isNavigating)
                return NavigationResult.OperationInProgress;

            if (_panelStack.Count == 0)
                return NavigationResult.StackEmpty;

            _isNavigating = true;
            var poppedPanel = _panelStack.Pop();
            OnNavigationStarted?.Invoke(poppedPanel);

            try
            {
                // Hide current panel
                await poppedPanel.HideAsync();
                _activePanels.Remove(poppedPanel);

                var previousPanel = _currentPanel;
                
                // Show previous panel if any
                if (_panelStack.Count > 0)
                {
                    _currentPanel = _panelStack.Peek();
                    await _currentPanel.ShowAsync();
                    _activePanels.Add(_currentPanel);
                }
                else
                {
                    _currentPanel = null;
                    OnStackEmpty?.Invoke();
                }

                // Fire events
                OnPanelPopped?.Invoke(poppedPanel);
                OnCurrentPanelChanged?.Invoke(previousPanel, _currentPanel);
                OnNavigationCompleted?.Invoke(poppedPanel);

                LogIfEnabled($"Popped panel: {poppedPanel.name}");
                return NavigationResult.Success;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Replaces the current panel with a new one (doesn't affect stack size)
        /// </summary>
        /// <param name="panelId">ID of the panel to replace with</param>
        /// <returns>Navigation result</returns>
        public async Task<NavigationResult> ReplacePanelAsync(string panelId)
        {
            if (_isNavigating)
                return NavigationResult.OperationInProgress;

            if (!_registeredPanels.TryGetValue(panelId, out var panel))
                return NavigationResult.PanelNotFound;

            if (_panelStack.Count == 0)
                return NavigationResult.StackEmpty;

            _isNavigating = true;
            OnNavigationStarted?.Invoke(panel);

            try
            {
                // Hide and remove current panel
                var oldPanel = _panelStack.Pop();
                await oldPanel.HideAsync();
                _activePanels.Remove(oldPanel);

                // Add and show new panel
                _panelStack.Push(panel);
                var previousPanel = _currentPanel;
                _currentPanel = panel;
                _activePanels.Add(panel);

                await panel.ShowAsync();

                // Fire events
                OnCurrentPanelChanged?.Invoke(previousPanel, _currentPanel);
                OnNavigationCompleted?.Invoke(panel);

                LogIfEnabled($"Replaced panel with: {panelId}");
                return NavigationResult.Success;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Clears the entire stack and optionally shows a new panel
        /// </summary>
        /// <param name="newPanelId">Optional panel to show after clearing</param>
        /// <returns>Navigation result</returns>
        public async Task<NavigationResult> ClearStackAsync(string newPanelId = null)
        {
            if (_isNavigating)
                return NavigationResult.OperationInProgress;

            _isNavigating = true;

            try
            {
                // Hide all active panels
                var hideTasks = _activePanels.Select(panel => panel.HideAsync()).ToArray();
                await Task.WhenAll(hideTasks);

                // Clear everything
                _panelStack.Clear();
                _activePanels.Clear();
                var previousPanel = _currentPanel;
                _currentPanel = null;

                OnCurrentPanelChanged?.Invoke(previousPanel, null);
                OnStackEmpty?.Invoke();

                // Show new panel if specified
                if (!string.IsNullOrEmpty(newPanelId))
                {
                    return await PushPanelAsync(newPanelId, false);
                }

                LogIfEnabled("Cleared panel stack");
                return NavigationResult.Success;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Shows a panel without affecting the stack
        /// </summary>
        /// <param name="panelId">ID of the panel to show</param>
        /// <returns>Navigation result</returns>
        public async Task<NavigationResult> ShowPanelAsync(string panelId)
        {
            if (!_registeredPanels.TryGetValue(panelId, out var panel))
                return NavigationResult.PanelNotFound;

            await panel.ShowAsync();
            _activePanels.Add(panel);
            
            LogIfEnabled($"Showed panel: {panelId}");
            return NavigationResult.Success;
        }

        /// <summary>
        /// Hides a panel without affecting the stack
        /// </summary>
        /// <param name="panelId">ID of the panel to hide</param>
        /// <returns>Navigation result</returns>
        public async Task<NavigationResult> HidePanelAsync(string panelId)
        {
            if (!_registeredPanels.TryGetValue(panelId, out var panel))
                return NavigationResult.PanelNotFound;

            await panel.HideAsync();
            _activePanels.Remove(panel);
            
            LogIfEnabled($"Hid panel: {panelId}");
            return NavigationResult.Success;
        }

        /// <summary>
        /// Hides all registered panels immediately
        /// </summary>
        public void HideAllPanelsImmediate()
        {
            foreach (var panel in _registeredPanels.Values)
            {
                panel.HideImmediate();
            }
            
            _activePanels.Clear();
            LogIfEnabled("Hid all panels immediately");
        }

        /// <summary>
        /// Gets a registered panel by ID
        /// </summary>
        /// <param name="panelId">ID of the panel to get</param>
        /// <returns>Panel instance or null if not found</returns>
        public UIPanel GetPanel(string panelId)
        {
            _registeredPanels.TryGetValue(panelId, out var panel);
            return panel;
        }

        /// <summary>
        /// Checks if a panel is currently in the stack
        /// </summary>
        /// <param name="panelId">ID of the panel to check</param>
        /// <returns>True if panel is in stack</returns>
        public bool IsInStack(string panelId)
        {
            if (!_registeredPanels.TryGetValue(panelId, out var panel))
                return false;

            return _panelStack.Contains(panel);
        }

        /// <summary>
        /// Gets the stack position of a panel (0 = top)
        /// </summary>
        /// <param name="panelId">ID of the panel to check</param>
        /// <returns>Stack position or -1 if not in stack</returns>
        public int GetStackPosition(string panelId)
        {
            if (!_registeredPanels.TryGetValue(panelId, out var panel))
                return -1;

            var stackArray = _panelStack.ToArray();
            for (int i = 0; i < stackArray.Length; i++)
            {
                if (stackArray[i] == panel)
                    return i;
            }

            return -1;
        }

        #endregion

        #region Private Methods

        private void HandleBackButtonInput()
        {
            // Handle Android back button or escape key
            if (Input.GetKeyDown(KeyCode.Escape) && CanGoBack && !_isNavigating)
            {
                _ = PopPanelAsync(); // Fire and forget
            }
        }

        private void RemoveFromStack(UIPanel panel)
        {
            if (_panelStack.Contains(panel))
            {
                var tempStack = new Stack<UIPanel>();
                
                while (_panelStack.Count > 0)
                {
                    var currentPanel = _panelStack.Pop();
                    if (currentPanel != panel)
                    {
                        tempStack.Push(currentPanel);
                    }
                }
                
                while (tempStack.Count > 0)
                {
                    _panelStack.Push(tempStack.Pop());
                }
            }
        }

        private void OnPanelShowCompleted(UIPanel panel)
        {
            // Panel show completed - could trigger additional logic here
        }

        private void OnPanelHideCompleted(UIPanel panel)
        {
            // Panel hide completed - could trigger additional logic here
        }

        private void LogIfEnabled(string message)
        {
            if (_logNavigationEvents)
            {
                Debug.Log($"[UIPanelStackManager] {message}", this);
            }
        }

        #endregion

        #region Public API - Synchronous Methods

        /// <summary>
        /// Pushes a panel onto the stack (fire and forget async operation)
        /// </summary>
        /// <param name="panelId">ID of the panel to push</param>
        /// <param name="hideCurrentPanel">Whether to hide the current panel</param>
        public void PushPanel(string panelId, bool hideCurrentPanel = true)
        {
            _ = PushPanelAsync(panelId, hideCurrentPanel);
        }

        /// <summary>
        /// Pops the current panel from the stack (fire and forget async operation)
        /// </summary>
        public void PopPanel()
        {
            _ = PopPanelAsync();
        }

        /// <summary>
        /// Replaces the current panel (fire and forget async operation)
        /// </summary>
        /// <param name="panelId">ID of the panel to replace with</param>
        public void ReplacePanel(string panelId)
        {
            _ = ReplacePanelAsync(panelId);
        }

        /// <summary>
        /// Clears the stack (fire and forget async operation)
        /// </summary>
        /// <param name="newPanelId">Optional panel to show after clearing</param>
        public void ClearStack(string newPanelId = null)
        {
            _ = ClearStackAsync(newPanelId);
        }

        #endregion
    }
} 