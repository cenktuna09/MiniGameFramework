using UnityEngine;
using Core.Common.ConfigManagement;

namespace EndlessRunner.Config
{
    /// <summary>
    /// Configuration for Endless Runner game
    /// Extends BaseGameConfig with runner-specific settings
    /// </summary>
    [CreateAssetMenu(fileName = "RunnerConfig", menuName = "EndlessRunner/Config/RunnerConfig")]
    public class RunnerConfig : BaseGameConfig
    {
        #region Player Settings
        [Header("Player Settings")]
        [SerializeField] private float _playerSpeed = 10f;
        [SerializeField] private float _lateralSpeed = 5f;
        [SerializeField] private float _jumpForce = 8f;
        [SerializeField] private float _slideDuration = 1f;
        [SerializeField] private float _dashSpeed = 15f;
        [SerializeField] private float _dashDuration = 0.5f;
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private bool _enableDoubleJump = true;
        [SerializeField] private bool _enableDoubleSlide = true;
        #endregion
        
        #region World Settings
        [Header("World Settings")]
        [SerializeField] private float _worldSpeed = 10f;
        [SerializeField] private float _chunkLength = 50f;
        [SerializeField] private int _maxChunks = 5;
        [SerializeField] private float _obstacleSpawnRate = 0.3f;
        [SerializeField] private float _collectibleSpawnRate = 0.5f;
        [SerializeField] private int _laneCount = 3;
        [SerializeField] private float _laneWidth = 2f;
        #endregion
        
        #region Scoring Settings
        [Header("Scoring Settings")]
        [SerializeField] private int _baseScorePerSecond = 10;
        [SerializeField] private int _collectibleValue = 100;
        [SerializeField] private int _obstacleAvoidBonus = 50;
        [SerializeField] private int _comboMultiplier = 2;
        [SerializeField] private float _comboTimeWindow = 2f;
        #endregion
        
        #region Difficulty Settings
        [Header("Difficulty Settings")]
        [SerializeField] private float _difficultyIncreaseRate = 0.1f;
        [SerializeField] private float _maxDifficulty = 5f;
        [SerializeField] private float _speedIncreaseRate = 0.05f;
        [SerializeField] private float _maxSpeed = 20f;
        #endregion
        
        #region Performance Settings
        [Header("Performance Settings")]
        [SerializeField] private int _maxObstacles = 100;
        [SerializeField] private int _maxCollectibles = 50;
        [SerializeField] private float _despawnDistance = 100f;
        [SerializeField] private bool _enableObjectPooling = true;
        #endregion
        
        #region Public Properties
        // Player Settings
        public float PlayerSpeed => _playerSpeed;
        public float LateralSpeed => _lateralSpeed;
        public float JumpForce => _jumpForce;
        public float SlideDuration => _slideDuration;
        public float DashSpeed => _dashSpeed;
        public float DashDuration => _dashDuration;
        public int MaxHealth => _maxHealth;
        public bool EnableDoubleJump => _enableDoubleJump;
        public bool EnableDoubleSlide => _enableDoubleSlide;
        
        // World Settings
        public float WorldSpeed => _worldSpeed;
        public float ChunkLength => _chunkLength;
        public int MaxChunks => _maxChunks;
        public float ObstacleSpawnRate => _obstacleSpawnRate;
        public float CollectibleSpawnRate => _collectibleSpawnRate;
        public int LaneCount => _laneCount;
        public float LaneWidth => _laneWidth;
        
        // Scoring Settings
        public int BaseScorePerSecond => _baseScorePerSecond;
        public int CollectibleValue => _collectibleValue;
        public int ObstacleAvoidBonus => _obstacleAvoidBonus;
        public int ComboMultiplier => _comboMultiplier;
        public float ComboTimeWindow => _comboTimeWindow;
        
        // Difficulty Settings
        public float DifficultyIncreaseRate => _difficultyIncreaseRate;
        public float MaxDifficulty => _maxDifficulty;
        public float SpeedIncreaseRate => _speedIncreaseRate;
        public float MaxSpeed => _maxSpeed;
        
        // Performance Settings
        public int MaxObstacles => _maxObstacles;
        public int MaxCollectibles => _maxCollectibles;
        public float DespawnDistance => _despawnDistance;
        public bool EnableObjectPooling => _enableObjectPooling;
        #endregion
        
        #region Abstract Method Implementations
        
        /// <summary>
        /// Validate all settings in the configuration
        /// </summary>
        public override void ValidateSettings()
        {
            // Validate base settings first
            ValidateBaseSettings();
            
            // Validate player settings
            if (_playerSpeed <= 0f)
            {
                Debug.LogError("[RunnerConfig] ❌ Player speed must be greater than 0");
            }
            
            if (_jumpForce <= 0f)
            {
                Debug.LogError("[RunnerConfig] ❌ Jump force must be greater than 0");
            }
            
            if (_slideDuration <= 0f)
            {
                Debug.LogError("[RunnerConfig] ❌ Slide duration must be greater than 0");
            }
            
            // Validate world settings
            if (_chunkLength <= 0f)
            {
                Debug.LogError("[RunnerConfig] ❌ Chunk length must be greater than 0");
            }
            
            if (_laneCount < 1)
            {
                Debug.LogError("[RunnerConfig] ❌ Lane count must be at least 1");
            }
            
            // Validate scoring settings
            if (_baseScorePerSecond < 0)
            {
                Debug.LogError("[RunnerConfig] ❌ Base score per second cannot be negative");
            }
            
            if (_collectibleValue < 0)
            {
                Debug.LogError("[RunnerConfig] ❌ Collectible value cannot be negative");
            }
            
            Debug.Log("[RunnerConfig] ✅ Configuration validated successfully");
        }
        
        /// <summary>
        /// Reset configuration to default values
        /// </summary>
        public override void ResetToDefaults()
        {
            // Reset base settings first
            ResetBaseToDefaults();
            
            // Reset player settings
            _playerSpeed = 10f;
            _lateralSpeed = 5f;
            _jumpForce = 8f;
            _slideDuration = 1f;
            _dashSpeed = 15f;
            _dashDuration = 0.5f;
            _maxHealth = 3;
            _enableDoubleJump = true;
            _enableDoubleSlide = true;
            
            // Reset world settings
            _worldSpeed = 10f;
            _chunkLength = 50f;
            _maxChunks = 5;
            _obstacleSpawnRate = 0.3f;
            _collectibleSpawnRate = 0.5f;
            _laneCount = 3;
            _laneWidth = 2f;
            
            // Reset scoring settings
            _baseScorePerSecond = 10;
            _collectibleValue = 100;
            _obstacleAvoidBonus = 50;
            _comboMultiplier = 2;
            _comboTimeWindow = 2f;
            
            // Reset difficulty settings
            _difficultyIncreaseRate = 0.1f;
            _maxDifficulty = 5f;
            _speedIncreaseRate = 0.05f;
            _maxSpeed = 20f;
            
            // Reset performance settings
            _maxObstacles = 100;
            _maxCollectibles = 50;
            _despawnDistance = 100f;
            _enableObjectPooling = true;
            
            Debug.Log("[RunnerConfig] 🔄 Configuration reset to defaults");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Get configuration summary
        /// </summary>
        public override string GetConfigurationString()
        {
            return $"Runner Configuration Summary:\n" +
                   $"Game Name: {GameName}\n" +
                   $"Version: {GameVersion}\n" +
                   $"Debug Mode: {EnableDebugMode}\n" +
                   $"Player Speed: {_playerSpeed}\n" +
                   $"Jump Force: {_jumpForce}\n" +
                   $"World Speed: {_worldSpeed}\n" +
                   $"Lane Count: {_laneCount}\n" +
                   $"Base Score/Second: {_baseScorePerSecond}";
        }
        
        #endregion
    }
} 