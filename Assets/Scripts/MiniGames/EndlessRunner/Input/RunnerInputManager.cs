using UnityEngine;
using Core.Common.InputManagement;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;
using EndlessRunner.StateManagement;
using Core.Common.StateManagement;
using UnityEngine.InputSystem;

namespace EndlessRunner.Input
{
    /// <summary>
    /// Input manager for Endless Runner game
    /// Handles 3D mouse/touch input including lateral movement, jumping, and sliding
    /// </summary>
    public class RunnerInputManager : BaseInputManager
    {
        #region Private Fields
        private RunnerInputHandler inputHandler;
        private Camera mainCamera;
        
        // Input state
        private bool isDragging = false;
        private Vector3 lastMousePosition;
        private Vector3 currentMousePosition;
        
        // Configuration
        private readonly float minSwipeDistance = 50f; // Minimum swipe distance for jump
        private readonly float movementSensitivity = 1f;
        
        // Input tracking
        private float _lateralInput = 0f;
        private bool _jumpPressed = false;
        private bool _slidePressed = false;
        private bool _dashPressed = false;
        
        // Input sensitivity and thresholds
        private float _lateralSensitivity = 1f;
        private float _inputThreshold = 0.1f;
        private float _doubleTapTime = 0.3f;
        
        // Input state tracking
        private float _lastJumpTime = 0f;
        private float _lastSlideTime = 0f;
        private float _lastDashTime = 0f;
        private float _lastLateralMovementTime = 0f; // Add lateral movement cooldown
        private bool _isDoubleJumpAvailable = true;
        private bool _isDoubleSlideAvailable = true;
        
        // Game state tracking
        private bool _gameStarted = false;
        private RunnerGameState _currentGameState = RunnerGameState.Ready;
        #endregion
        
        #region Public Properties
        public float LateralInput => _lateralInput;
        public bool IsJumpPressed => _jumpPressed;
        public bool IsSlidePressed => _slidePressed;
        public bool IsDashPressed => _dashPressed;
        public float LateralSensitivity
        {
            get => _lateralSensitivity;
            set => _lateralSensitivity = Mathf.Clamp(value, 0.1f, 5f);
        }
        #endregion
        
        #region Constructor
        public RunnerInputManager(IEventBus eventBus) : base(eventBus)
        {
            this.mainCamera = Camera.main;
            
            Initialize();
            
            Debug.Log("[RunnerInputManager] ✅ 3D Input manager initialized");
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Process input for the current frame
        /// </summary>
        public override void ProcessInput()
        {
            if (_isInputLocked) return;
            
            // Check if player is dead - if so, don't process input
            var playerController = Object.FindFirstObjectByType<EndlessRunner.Player.PlayerController>();
            if (playerController != null && playerController.IsDead)
            {
                return; // Don't process input when player is dead
            }
            
            var inputResult = inputHandler?.ProcessInput() ?? ProcessInputDirectly();
            if (inputResult.HasInput)
            {
                // Handle first tap to start the game
                if (!_gameStarted && inputResult.InputStarted)
                {
                    StartGame();
                }
                
                var command = CreateCommandFromInput(inputResult);
                HandleInputCommand(command);
            }
        }
        
        /// <summary>
        /// Set input sensitivity for lateral movement
        /// </summary>
        public void SetLateralSensitivity(float sensitivity)
        {
            _lateralSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
            Debug.Log($"[RunnerInputManager] 🎛️ Lateral sensitivity set to: {_lateralSensitivity}");
        }
        
        /// <summary>
        /// Reset input state
        /// </summary>
        public void ResetInputState()
        {
            _lateralInput = 0f;
            _jumpPressed = false;
            _slidePressed = false;
            _dashPressed = false;
            _lastJumpTime = 0f;
            _lastSlideTime = 0f;
            _lastDashTime = 0f;
            _lastLateralMovementTime = 0f; // Reset lateral movement cooldown
            _isDoubleJumpAvailable = true;
            _isDoubleSlideAvailable = true;
            isDragging = false;
            
            Debug.Log("[RunnerInputManager] 🔄 Input state reset");
        }
        
        /// <summary>
        /// Start the game on first tap
        /// </summary>
        private void StartGame()
        {
            if (_gameStarted) return;
            
            _gameStarted = true;
            _currentGameState = RunnerGameState.Running;
            
            // Publish game started event
            var gameStartedEvent = new StateChangedEvent<RunnerGameState>(RunnerGameState.Ready, RunnerGameState.Running);
            _eventBus?.Publish(gameStartedEvent);
            
            Debug.Log("[RunnerInputManager] 🚀 Game started on first tap!");
        }
        
        /// <summary>
        /// Subscribe to game state changes
        /// </summary>
        private void SubscribeToGameState()
        {
            _eventBus?.Subscribe<StateChangedEvent<RunnerGameState>>(OnGameStateChanged);
        }
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(StateChangedEvent<RunnerGameState> stateEvent)
        {
            _currentGameState = stateEvent.NewState;
            
            if (stateEvent.NewState == RunnerGameState.GameOver)
            {
                // Reset game state when game over
                _gameStarted = false;
                ResetInputState();
            }
            
            Debug.Log($"[RunnerInputManager] 🔄 Game state changed to: {stateEvent.NewState}");
        }
        #endregion
        
        #region Protected Methods
        /// <summary>
        /// Handle input commands
        /// </summary>
        protected override void HandleInputCommand(BaseInputCommand command)
        {
            if (command is RunnerInputCommand runnerCommand)
            {
                runnerCommand.Execute();
                
                // Publish input event
                var inputEvent = new RunnerInputEvent(runnerCommand.CommandType, runnerCommand.CommandData);
                _eventBus.Publish(inputEvent);
                
                Debug.Log($"[RunnerInputManager] 🎮 Executed command: {runnerCommand.CommandType}");
            }
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initialize the input manager
        /// </summary>
        protected override void Initialize()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogWarning("[RunnerInputManager] ⚠️ No main camera found!");
                }
            }
            
            // Subscribe to game state changes
            SubscribeToGameState();
        }
        
        /// <summary>
        /// Process input directly using mouse/touch
        /// </summary>
        private InputResult ProcessInputDirectly()
        {
            var result = new InputResult();
            
            // Mouse/Touch Down - Start tracking
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = UnityEngine.Input.mousePosition;
                currentMousePosition = UnityEngine.Input.mousePosition;
                
                result.InputStarted = true;
                result.HasInput = true;
                result.StartPosition = GetWorldPosition(lastMousePosition);
                
                Debug.Log($"[RunnerInputManager] 🎯 Input started at: {result.StartPosition}");
            }
            
            // Mouse/Touch Up - End tracking
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                if (isDragging)
                {
                    var endPosition = GetWorldPosition(UnityEngine.Input.mousePosition);
                    var swipeDistance = Vector3.Distance(lastMousePosition, UnityEngine.Input.mousePosition);
                    
                                         // Check for jump gesture (vertical swipe)
                     if (swipeDistance > minSwipeDistance)
                     {
                         var verticalDelta = UnityEngine.Input.mousePosition.y - lastMousePosition.y;
                         var horizontalDelta = UnityEngine.Input.mousePosition.x - lastMousePosition.x;
                         
                         if (Mathf.Abs(verticalDelta) > minSwipeDistance)
                         {
                             result.JumpDetected = true;
                             result.JumpDirection = verticalDelta > 0 ? JumpDirection.Up : JumpDirection.Down;
                             
                             Debug.Log($"[RunnerInputManager] 🦘 Jump detected: {result.JumpDirection}");
                         }
                         // Check for slide gesture (horizontal swipe down)
                         else if (Mathf.Abs(horizontalDelta) > minSwipeDistance && verticalDelta < -minSwipeDistance)
                         {
                             result.SlideDetected = true;
                             
                             Debug.Log($"[RunnerInputManager] 🛷 Slide detected!");
                         }
                     }
                    
                    result.InputEnded = true;
                    result.HasInput = true;
                    result.EndPosition = endPosition;
                    
                    Debug.Log($"[RunnerInputManager] 🎯 Input ended at: {endPosition}");
                }
                
                isDragging = false;
            }
            
            // Continuous movement while dragging
            if (isDragging)
            {
                currentMousePosition = UnityEngine.Input.mousePosition;
                var worldPos = GetWorldPosition(currentMousePosition);
                var lastWorldPos = GetWorldPosition(lastMousePosition);
                
                // Only process movement if there's significant horizontal movement
                var movementDelta = worldPos - lastWorldPos;
                if (Mathf.Abs(movementDelta.x) > _inputThreshold)
                {
                    result.MovementDetected = true;
                    result.HasInput = true;
                    result.CurrentPosition = worldPos;
                    result.MovementDelta = movementDelta;
                    
                    // Publish continuous movement event
                    _eventBus?.Publish(new PlayerMovementEvent(worldPos, movementDelta, 0f, 0f));
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Convert screen position to world position
        /// </summary>
        private Vector3 GetWorldPosition(Vector3 screenPosition)
        {
            if (mainCamera == null) return Vector3.zero;
            
            // Get player's current Z position for consistent world coordinates
            var playerController = Object.FindFirstObjectByType<EndlessRunner.Player.PlayerController>();
            float playerZ = playerController != null ? playerController.transform.position.z : 0f;
            
            var ray = mainCamera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, new Vector3(0f, 0f, playerZ));
            
            if (plane.Raycast(ray, out float distance))
            {
                var worldPoint = ray.GetPoint(distance);
                // Only use X and Z coordinates, ignore Y (vertical) movement for lateral input
                return new Vector3(worldPoint.x, 0f, worldPoint.z);
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// Create command from input result
        /// </summary>
        private RunnerInputCommand CreateCommandFromInput(InputResult inputResult)
        {
            if (inputResult.JumpDetected)
            {
                float currentTime = Time.time;
                bool isDoubleJump = (currentTime - _lastJumpTime) < _doubleTapTime && _isDoubleJumpAvailable;
                
                _lastJumpTime = currentTime;
                if (isDoubleJump)
                {
                    _isDoubleJumpAvailable = false;
                }
                
                _jumpPressed = true;
                
                // Get player's actual position for jump event
                var playerController = Object.FindFirstObjectByType<EndlessRunner.Player.PlayerController>();
                Vector3 playerPosition = playerController != null ? playerController.transform.position : Vector3.zero;
                
                // Publish jump event
                var jumpEvent = new PlayerJumpEvent(playerPosition, 0f, 0f, isDoubleJump);
                _eventBus.Publish(jumpEvent);
                
                return new RunnerInputCommand(
                    RunnerInputType.Jump,
                    new RunnerInputData { IsDoubleJump = isDoubleJump }
                );
            }
            
            if (inputResult.SlideDetected)
            {
                float currentTime = Time.time;
                bool isDoubleSlide = (currentTime - _lastSlideTime) < _doubleTapTime && _isDoubleSlideAvailable;
                
                _lastSlideTime = currentTime;
                if (isDoubleSlide)
                {
                    _isDoubleSlideAvailable = false;
                }
                
                _slidePressed = true;
                
                // Get player's actual position for slide event
                var playerController = Object.FindFirstObjectByType<EndlessRunner.Player.PlayerController>();
                Vector3 playerPosition = playerController != null ? playerController.transform.position : Vector3.zero;
                
                // Publish slide event
                var slideEvent = new PlayerSlideEvent(playerPosition, 0f, 0f, true);
                _eventBus.Publish(slideEvent);
                
                return new RunnerInputCommand(
                    RunnerInputType.Slide,
                    new RunnerInputData { IsDoubleSlide = isDoubleSlide }
                );
            }
            
            if (inputResult.MovementDetected)
            {
                // Calculate lateral input based on horizontal movement only
                var lateralDelta = inputResult.MovementDelta.x * movementSensitivity;
                _lateralInput = Mathf.Clamp(lateralDelta, -1f, 1f);
                
                // Only process lateral movement if horizontal movement is significant
                if (Mathf.Abs(_lateralInput) > _inputThreshold)
                {
                    // Add cooldown for lateral movement to prevent multiple lane changes
                    float currentTime = Time.time;
                    float lateralCooldown = 0.3f; // 300ms cooldown between lane changes
                    
                    if (currentTime - _lastLateralMovementTime > lateralCooldown)
                    {
                        _lastLateralMovementTime = currentTime;
                        
                        // Get player's actual position for lateral movement event
                        var playerController = Object.FindFirstObjectByType<EndlessRunner.Player.PlayerController>();
                        Vector3 playerPosition = playerController != null ? playerController.transform.position : Vector3.zero;
                        
                        // Publish lateral movement event
                        var lateralEvent = new PlayerLateralMovementEvent(_lateralInput, playerPosition, 0f);
                        _eventBus.Publish(lateralEvent);
                        
                        Debug.Log($"[RunnerInputManager] 🛣️ Lateral movement: {_lateralInput:F2} (cooldown applied)");
                        
                        return new RunnerInputCommand(
                            RunnerInputType.LateralMovement,
                            new RunnerInputData { LateralInput = _lateralInput }
                        );
                    }
                    else
                    {
                        //Debug.Log($"[RunnerInputManager] ⏳ Lateral movement on cooldown: {lateralCooldown - (currentTime - _lastLateralMovementTime):F2}s remaining");
                    }
                }
            }
            
            return null;
        }
        #endregion
    }
    
    #region Input Types and Data
    /// <summary>
    /// Types of input commands for the runner
    /// </summary>
    public enum RunnerInputType
    {
        LateralMovement,
        Jump,
        Slide,
        Dash,
        Pause,
        Resume
    }
    
    /// <summary>
    /// Data structure for runner input commands
    /// </summary>
    public struct RunnerInputData
    {
        public float LateralInput;
        public bool IsDoubleJump;
        public bool IsDoubleSlide;
        public Vector3 Direction;
        public float Intensity;
    }
    
    /// <summary>
    /// Input command specific to the runner game
    /// </summary>
    public class RunnerInputCommand : BaseInputCommand
    {
        public RunnerInputType CommandType { get; private set; }
        public RunnerInputData CommandData { get; private set; }
        
        public RunnerInputCommand(RunnerInputType commandType, RunnerInputData commandData)
        {
            CommandType = commandType;
            CommandData = commandData;
        }
        
        public override void Execute()
        {
            // Command execution logic will be handled by the input manager
            Debug.Log($"[RunnerInputCommand] 🎮 Executing {CommandType}");
        }
    }
    
    /// <summary>
    /// Event fired when runner input is detected
    /// </summary>
    public class RunnerInputEvent : GameEvent
    {
        public RunnerInputType InputType { get; private set; }
        public RunnerInputData InputData { get; private set; }
        
        public RunnerInputEvent(RunnerInputType inputType, RunnerInputData inputData, GameObject source = null) : base(source)
        {
            InputType = inputType;
            InputData = inputData;
        }
    }
    
    /// <summary>
    /// Input result structure for mouse/touch input
    /// </summary>
    public struct InputResult
    {
        public bool HasInput;
        public bool InputStarted;
        public bool InputEnded;
        public bool MovementDetected;
        public bool JumpDetected;
        public bool SlideDetected;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 CurrentPosition;
        public Vector3 MovementDelta;
        public JumpDirection JumpDirection;
    }
    
    /// <summary>
    /// Jump direction enum
    /// </summary>
    public enum JumpDirection
    {
        Up,
        Down
    }
    
    /// <summary>
    /// Input handler interface for extensibility
    /// </summary>
    public interface RunnerInputHandler
    {
        InputResult ProcessInput();
    }
    #endregion
} 