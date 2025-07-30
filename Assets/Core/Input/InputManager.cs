using System;
using UnityEngine;
using UnityEngine.InputSystem;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniGameFramework.Core.Input
{
    /// <summary>
    /// Main input manager that bridges Unity Input System with our event system.
    /// Handles input context switching and event distribution.
    /// </summary>
    public class InputManager : MonoBehaviour, IInputManager
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float tapMaxDuration = 0.3f;

        private IEventBus eventBus;
        private InputContext currentContext = InputContext.Menu;
        private bool isInputEnabled = true;
        
        // Input Action Maps
        private InputActionMap uiActionMap;
        private InputActionMap playerActionMap;
        
        // Swipe detection
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isTouchActive;

        public bool IsInputEnabled => isInputEnabled;
        public InputContext CurrentContext => currentContext;

        private void Awake()
        {
            if (inputActions == null)
            {
                TryAutoAssignInputActions();
            }

            if (inputActions == null)
            {
                Debug.LogError("InputManager: No InputActionAsset found! Please assign one in inspector.");
                return;
            }

            InitializeActionMaps();
        }

        private void TryAutoAssignInputActions()
        {
            // First try to load from Resources
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            
            if (inputActions == null)
            {
                // Try common asset names
                string[] commonNames = { "InputActions", "PlayerInput", "GameInput", "Input" };
                foreach (string name in commonNames)
                {
                    inputActions = Resources.Load<InputActionAsset>(name);
                    if (inputActions != null)
                    {
                        Debug.LogWarning($"InputManager: Auto-assigned InputActionAsset from Resources: {inputActions.name}");
                        break;
                    }
                }
            }

#if UNITY_EDITOR
            // As last resort in editor, search project files
            if (inputActions == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:InputActionAsset");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    inputActions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                    Debug.LogWarning($"InputManager: Auto-assigned InputActionAsset from project: {inputActions.name}");
                }
            }
#endif
        }

        public void Initialize(InputContext context = InputContext.Menu)
        {
            eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            
            if (eventBus == null)
            {
                Debug.LogError("InputManager: EventBus service not found!");
                return;
            }

            SwitchContext(context);
            SetupInputCallbacks();
            
            Debug.Log($"InputManager initialized with context: {context}");
        }

        public void SwitchContext(InputContext context)
        {
            DisableAllActionMaps();
            currentContext = context;

            switch (context)
            {
                case InputContext.Menu:
                    uiActionMap?.Enable();
                    break;
                    
                case InputContext.Match3:
                    uiActionMap?.Enable();
                    EnableMatch3Input();
                    break;
                    
                case InputContext.EndlessRunner:
                    playerActionMap?.Enable();
                    EnableEndlessRunnerInput();
                    break;
                    
                case InputContext.Paused:
                    uiActionMap?.Enable();
                    break;
            }

            Debug.Log($"InputManager: Switched to context {context}");
        }

        public void EnableInput()
        {
            isInputEnabled = true;
            inputActions?.Enable();
        }

        public void DisableInput()
        {
            isInputEnabled = false;
            inputActions?.Disable();
        }

        public void SetInputMapEnabled(string mapName, bool enabled)
        {
            var actionMap = inputActions?.FindActionMap(mapName);
            if (actionMap != null)
            {
                if (enabled)
                    actionMap.Enable();
                else
                    actionMap.Disable();
            }
        }

        public Vector2 GetPointerPosition()
        {
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }

        public bool IsActionPressed(string actionName)
        {
            var action = inputActions?.FindAction(actionName);
            return action?.IsPressed() ?? false;
        }

        public float GetActionValue(string actionName)
        {
            var action = inputActions?.FindAction(actionName);
            return action?.ReadValue<float>() ?? 0f;
        }

        public void Dispose()
        {
            DisableAllActionMaps();
            // InputActionAsset doesn't have Dispose, just disable
            inputActions?.Disable();
        }

        private void InitializeActionMaps()
        {
            uiActionMap = inputActions.FindActionMap("UI");
            playerActionMap = inputActions.FindActionMap("Player");
            
            if (uiActionMap == null)
                Debug.LogWarning("InputManager: UI action map not found!");
            if (playerActionMap == null)
                Debug.LogWarning("InputManager: Player action map not found!");
        }

        private void SetupInputCallbacks()
        {
            SetupUICallbacks();
            SetupPlayerCallbacks();
        }

        private void SetupUICallbacks()
        {
            if (uiActionMap == null) return;

            // UI Navigation
            var navigateAction = uiActionMap.FindAction("Navigate");
            if (navigateAction != null)
                navigateAction.performed += OnUINavigate;

            // UI Actions
            var submitAction = uiActionMap.FindAction("Submit");
            if (submitAction != null)
                submitAction.performed += OnUISubmit;

            var cancelAction = uiActionMap.FindAction("Cancel");
            if (cancelAction != null)
                cancelAction.performed += OnUICancel;

            // Pointer Actions
            var clickAction = uiActionMap.FindAction("Click");
            if (clickAction != null)
                clickAction.performed += OnUIClick;

            var pointAction = uiActionMap.FindAction("Point");
            if (pointAction != null)
                pointAction.performed += OnUIPoint;
        }

        private void SetupPlayerCallbacks()
        {
            if (playerActionMap == null) return;

            // Player Actions
            var jumpAction = playerActionMap.FindAction("Jump");
            if (jumpAction != null)
                jumpAction.performed += OnJump;

            var moveAction = playerActionMap.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }

            var attackAction = playerActionMap.FindAction("Attack");
            if (attackAction != null)
                attackAction.performed += OnAttack;

            var crouchAction = playerActionMap.FindAction("Crouch");
            if (crouchAction != null)
            {
                crouchAction.performed += OnCrouch;
                crouchAction.canceled += OnCrouch;
            }
        }

        private void EnableMatch3Input()
        {
            // For Match3, we mainly use touch/mouse input handled through UI system
            // Additional swipe detection handled in Update
        }

        private void EnableEndlessRunnerInput()
        {
            // Endless Runner uses player action map which is already enabled
        }

        private void DisableAllActionMaps()
        {
            uiActionMap?.Disable();
            playerActionMap?.Disable();
        }

        #region Input Callbacks

        private void OnUINavigate(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            
            var direction = context.ReadValue<Vector2>();
            eventBus?.Publish(new UINavigationInputEvent(direction));
        }

        private void OnUISubmit(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            eventBus?.Publish(new UIActionInputEvent(UIActionInputEvent.UIActionType.Submit));
        }

        private void OnUICancel(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            eventBus?.Publish(new UIActionInputEvent(UIActionInputEvent.UIActionType.Cancel));
        }

        private void OnUIClick(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            
            var pointerPos = GetPointerPosition();
            eventBus?.Publish(new UIActionInputEvent(UIActionInputEvent.UIActionType.Click, pointerPos));

            // Convert to world position for Match3
            if (currentContext == InputContext.Match3)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(pointerPos);
                eventBus?.Publish(new TileClickInputEvent(pointerPos, worldPos));
            }
        }

        private void OnUIPoint(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            // This is handled continuously, no need for events on every frame
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            
            if (currentContext == InputContext.EndlessRunner)
            {
                eventBus?.Publish(new PlayerActionInputEvent(PlayerActionInputEvent.ActionType.Jump, true));
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            
            var moveDir = context.ReadValue<Vector2>();
            var isPressed = !context.canceled;
            
            eventBus?.Publish(new PlayerMoveInputEvent(moveDir, isPressed));

            // Handle lane changes for Endless Runner
            if (currentContext == InputContext.EndlessRunner && context.performed)
            {
                if (moveDir.x > 0.5f)
                    eventBus?.Publish(new LaneChangeInputEvent(LaneChangeInputEvent.LaneDirection.Right));
                else if (moveDir.x < -0.5f)
                    eventBus?.Publish(new LaneChangeInputEvent(LaneChangeInputEvent.LaneDirection.Left));
            }
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            eventBus?.Publish(new PlayerActionInputEvent(PlayerActionInputEvent.ActionType.Attack, true));
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            
            var isPressed = !context.canceled;
            eventBus?.Publish(new PlayerActionInputEvent(PlayerActionInputEvent.ActionType.Crouch, isPressed));
            
            // Handle slide for Endless Runner
            if (currentContext == InputContext.EndlessRunner && isPressed)
            {
                eventBus?.Publish(new SlideInputEvent(true));
            }
        }

        #endregion

        private void Update()
        {
            if (!isInputEnabled) return;
            HandleSwipeDetection();
        }

        private void HandleSwipeDetection()
        {
            if (currentContext != InputContext.Match3) return;

            // Touch input
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touch = Touchscreen.current.primaryTouch;
                var touchPos = touch.position.ReadValue();

                if (!isTouchActive)
                {
                    touchStartPosition = touchPos;
                    touchStartTime = Time.time;
                    isTouchActive = true;
                }
            }
            else if (isTouchActive)
            {
                var touch = Touchscreen.current?.primaryTouch;
                if (touch != null)
                {
                    var endPos = touch.position.ReadValue();
                    ProcessSwipe(touchStartPosition, endPos);
                }
                isTouchActive = false;
            }

            // Mouse input (for testing)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                touchStartPosition = Mouse.current.position.ReadValue();
                touchStartTime = Time.time;
                isTouchActive = true;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isTouchActive)
            {
                var endPos = Mouse.current.position.ReadValue();
                ProcessSwipe(touchStartPosition, endPos);
                isTouchActive = false;
            }
        }

        private void ProcessSwipe(Vector2 startPos, Vector2 endPos)
        {
            var distance = Vector2.Distance(startPos, endPos);
            var duration = Time.time - touchStartTime;

            if (distance >= swipeThreshold && duration <= tapMaxDuration * 2)
            {
                eventBus?.Publish(new SwipeInputEvent(startPos, endPos));
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}