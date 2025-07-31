using System;
using System.Threading.Tasks;

namespace MiniGameFramework.Core.SaveSystem
{
    /// <summary>
    /// Defines the contract for the save system.
    /// Provides data persistence functionality for the framework.
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// Save data to persistent storage.
        /// </summary>
        /// <typeparam name="T">The type of data to save.</typeparam>
        /// <param name="key">The key to save the data under.</param>
        /// <param name="data">The data to save.</param>
        /// <returns>True if save was successful, false otherwise.</returns>
        Task<bool> SaveAsync<T>(string key, T data);
        
        /// <summary>
        /// Save data to persistent storage synchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to save.</typeparam>
        /// <param name="key">The key to save the data under.</param>
        /// <param name="data">The data to save.</param>
        /// <returns>True if save was successful, false otherwise.</returns>
        bool Save<T>(string key, T data);
        
        /// <summary>
        /// Load data from persistent storage.
        /// </summary>
        /// <typeparam name="T">The type of data to load.</typeparam>
        /// <param name="key">The key to load the data from.</param>
        /// <param name="defaultValue">Default value to return if data doesn't exist.</param>
        /// <returns>The loaded data, or the default value if not found.</returns>
        Task<T> LoadAsync<T>(string key, T defaultValue = default);
        
        /// <summary>
        /// Load data from persistent storage synchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to load.</typeparam>
        /// <param name="key">The key to load the data from.</param>
        /// <param name="defaultValue">Default value to return if data doesn't exist.</param>
        /// <returns>The loaded data, or the default value if not found.</returns>
        T Load<T>(string key, T defaultValue = default);
        
        /// <summary>
        /// Check if data exists for a given key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if data exists, false otherwise.</returns>
        bool HasData(string key);
        
        /// <summary>
        /// Delete data for a given key.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        Task<bool> DeleteAsync(string key);
        
        /// <summary>
        /// Clear all saved data.
        /// </summary>
        /// <returns>True if clear was successful, false otherwise.</returns>
        Task<bool> ClearAllAsync();
        
        /// <summary>
        /// Get all saved keys.
        /// </summary>
        /// <returns>Array of all saved keys.</returns>
        string[] GetAllKeys();
        
        /// <summary>
        /// Event fired when data is saved.
        /// </summary>
        event Action<string> OnDataSaved;
        
        /// <summary>
        /// Event fired when data is loaded.
        /// </summary>
        event Action<string> OnDataLoaded;
        
        /// <summary>
        /// Event fired when data is deleted.
        /// </summary>
        event Action<string> OnDataDeleted;
    }
    
    /// <summary>
    /// Save data for high scores.
    /// </summary>
    [Serializable]
    public class HighScoreData
    {
        public string GameId { get; set; }
        public int Score { get; set; }
        public DateTime Timestamp { get; set; }
        public string PlayerName { get; set; }
        
        public HighScoreData()
        {
            Timestamp = DateTime.Now;
        }
        
        public HighScoreData(string gameId, int score, string playerName = "Player")
        {
            GameId = gameId;
            Score = score;
            PlayerName = playerName;
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Save data for game settings.
    /// </summary>
    [Serializable]
    public class GameSettingsData
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.8f;
        public float SFXVolume { get; set; } = 1.0f;
        public bool Fullscreen { get; set; } = true;
        public int QualityLevel { get; set; } = 2;
        public string Language { get; set; } = "en";
        
        public GameSettingsData()
        {
        }
    }
    
    /// <summary>
    /// Save data for player progress.
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        public string GameId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public DateTime LastPlayed { get; set; }
        public int TotalPlayTime { get; set; } // in seconds
        
        public PlayerProgressData()
        {
            LastPlayed = DateTime.Now;
        }
        
        public PlayerProgressData(string gameId)
        {
            GameId = gameId;
            Level = 1;
            Experience = 0;
            LastPlayed = DateTime.Now;
            TotalPlayTime = 0;
        }
    }
}