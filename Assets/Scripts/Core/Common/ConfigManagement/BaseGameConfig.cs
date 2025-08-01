using UnityEngine;

namespace Core.Common.ConfigManagement
{
    /// <summary>
    /// Base class for game configuration using ScriptableObject
    /// Provides common configuration properties and validation methods
    /// </summary>
    public abstract class BaseGameConfig : ScriptableObject
    {
        #region Header Configuration
        
        [Header("Base Configuration")]
        [SerializeField] protected string _gameName = "Game";
        [SerializeField] protected string _gameVersion = "1.0.0";
        [SerializeField] protected bool _enableDebugMode = false;
        [SerializeField] protected bool _enableLogging = true;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Name of the game
        /// </summary>
        public string GameName => _gameName;
        
        /// <summary>
        /// Version of the game
        /// </summary>
        public string GameVersion => _gameVersion;
        
        /// <summary>
        /// Whether debug mode is enabled
        /// </summary>
        public bool EnableDebugMode => _enableDebugMode;
        
        /// <summary>
        /// Whether logging is enabled
        /// </summary>
        public bool EnableLogging => _enableLogging;
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Validate all settings in the configuration
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void ValidateSettings();
        
        /// <summary>
        /// Reset configuration to default values
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void ResetToDefaults();
        
        #endregion
        
        #region Virtual Methods
        
        /// <summary>
        /// Initialize configuration with default values
        /// </summary>
        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(_gameName))
            {
                _gameName = "Game";
            }
            
            if (string.IsNullOrEmpty(_gameVersion))
            {
                _gameVersion = "1.0.0";
            }
            
            Debug.Log($"[{GetType().Name}] ✅ Configuration loaded: {_gameName} v{_gameVersion}");
        }
        
        /// <summary>
        /// Validate base configuration settings
        /// </summary>
        protected virtual void ValidateBaseSettings()
        {
            if (string.IsNullOrEmpty(_gameName))
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ Game name is empty");
            }
            
            if (string.IsNullOrEmpty(_gameVersion))
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ Game version is empty");
            }
            
            Debug.Log($"[{GetType().Name}] ✅ Base settings validated");
        }
        
        /// <summary>
        /// Reset base configuration to defaults
        /// </summary>
        protected virtual void ResetBaseToDefaults()
        {
            _gameName = "Game";
            _gameVersion = "1.0.0";
            _enableDebugMode = false;
            _enableLogging = true;
            
            Debug.Log($"[{GetType().Name}] 🔄 Base settings reset to defaults");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Get configuration as string for debugging
        /// </summary>
        /// <returns>Configuration string</returns>
        public virtual string GetConfigurationString()
        {
            return $"Game: {_gameName}, Version: {_gameVersion}, Debug: {_enableDebugMode}, Logging: {_enableLogging}";
        }
        
        /// <summary>
        /// Check if configuration is valid
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(_gameName) && !string.IsNullOrEmpty(_gameVersion);
        }
        
        #endregion
    }
} 