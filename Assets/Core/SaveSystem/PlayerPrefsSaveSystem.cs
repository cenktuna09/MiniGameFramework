using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGameFramework.Core.SaveSystem
{
    /// <summary>
    /// PlayerPrefs-based implementation of the save system.
    /// Provides simple, reliable data persistence using Unity's PlayerPrefs.
    /// </summary>
    public class PlayerPrefsSaveSystem : ISaveSystem
    {
        private const string KEY_PREFIX = "MiniGameFramework_";
        private readonly HashSet<string> knownKeys = new HashSet<string>();
        
        public event Action<string> OnDataSaved;
        public event Action<string> OnDataLoaded;
        public event Action<string> OnDataDeleted;
        
        public PlayerPrefsSaveSystem()
        {
            // Load known keys from PlayerPrefs
            LoadKnownKeys();
        }
        
        /// <summary>
        /// Save data to PlayerPrefs using JSON serialization.
        /// </summary>
        public async Task<bool> SaveAsync<T>(string key, T data)
        {
            try
            {
                var fullKey = GetFullKey(key);
                var json = JsonUtility.ToJson(data);
                
                PlayerPrefs.SetString(fullKey, json);
                PlayerPrefs.Save();
                
                knownKeys.Add(key);
                SaveKnownKeys();
                
                OnDataSaved?.Invoke(key);
                
                Debug.Log($"[SaveSystem] Saved data for key: {key}");
                await Task.CompletedTask;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save data for key {key}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Load data from PlayerPrefs using JSON deserialization.
        /// </summary>
        public async Task<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            try
            {
                var fullKey = GetFullKey(key);
                
                if (!PlayerPrefs.HasKey(fullKey))
                {
                    Debug.Log($"[SaveSystem] No data found for key: {key}");
                    await Task.CompletedTask;
                    return defaultValue;
                }
                
                var json = PlayerPrefs.GetString(fullKey);
                var data = JsonUtility.FromJson<T>(json);
                
                OnDataLoaded?.Invoke(key);
                
                Debug.Log($"[SaveSystem] Loaded data for key: {key}");
                await Task.CompletedTask;
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load data for key {key}: {e.Message}");
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Check if data exists for a given key.
        /// </summary>
        public bool HasData(string key)
        {
            var fullKey = GetFullKey(key);
            return PlayerPrefs.HasKey(fullKey);
        }
        
        /// <summary>
        /// Delete data for a given key.
        /// </summary>
        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                var fullKey = GetFullKey(key);
                
                if (PlayerPrefs.HasKey(fullKey))
                {
                    PlayerPrefs.DeleteKey(fullKey);
                    PlayerPrefs.Save();
                    
                    knownKeys.Remove(key);
                    SaveKnownKeys();
                    
                    OnDataDeleted?.Invoke(key);
                    
                    Debug.Log($"[SaveSystem] Deleted data for key: {key}");
                    await Task.CompletedTask;
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete data for key {key}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Clear all saved data.
        /// </summary>
        public async Task<bool> ClearAllAsync()
        {
            try
            {
                foreach (var key in knownKeys)
                {
                    var fullKey = GetFullKey(key);
                    if (PlayerPrefs.HasKey(fullKey))
                    {
                        PlayerPrefs.DeleteKey(fullKey);
                    }
                }
                
                PlayerPrefs.Save();
                knownKeys.Clear();
                SaveKnownKeys();
                
                Debug.Log("[SaveSystem] Cleared all saved data");
                await Task.CompletedTask;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to clear all data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get all saved keys.
        /// </summary>
        public string[] GetAllKeys()
        {
            var keysArray = new string[knownKeys.Count];
            knownKeys.CopyTo(keysArray);
            return keysArray;
        }
        
        #region High Score Methods
        
        /// <summary>
        /// Save a high score for a specific game.
        /// </summary>
        public async Task<bool> SaveHighScoreAsync(string gameId, int score, string playerName = "Player")
        {
            var highScoreData = new HighScoreData(gameId, score, playerName);
            var key = $"HighScore_{gameId}";
            return await SaveAsync(key, highScoreData);
        }
        
        /// <summary>
        /// Load the high score for a specific game.
        /// </summary>
        public async Task<HighScoreData> LoadHighScoreAsync(string gameId)
        {
            var key = $"HighScore_{gameId}";
            return await LoadAsync(key, new HighScoreData(gameId, 0));
        }
        
        /// <summary>
        /// Check if a score is a new high score for a game.
        /// </summary>
        public async Task<bool> IsNewHighScoreAsync(string gameId, int score)
        {
            var currentHighScore = await LoadHighScoreAsync(gameId);
            return score > currentHighScore.Score;
        }
        
        #endregion
        
        #region Settings Methods
        
        /// <summary>
        /// Save game settings.
        /// </summary>
        public async Task<bool> SaveSettingsAsync(GameSettingsData settings)
        {
            return await SaveAsync("GameSettings", settings);
        }
        
        /// <summary>
        /// Load game settings.
        /// </summary>
        public async Task<GameSettingsData> LoadSettingsAsync()
        {
            return await LoadAsync("GameSettings", new GameSettingsData());
        }
        
        #endregion
        
        #region Progress Methods
        
        /// <summary>
        /// Save player progress for a specific game.
        /// </summary>
        public async Task<bool> SaveProgressAsync(string gameId, PlayerProgressData progress)
        {
            var key = $"Progress_{gameId}";
            return await SaveAsync(key, progress);
        }
        
        /// <summary>
        /// Load player progress for a specific game.
        /// </summary>
        public async Task<PlayerProgressData> LoadProgressAsync(string gameId)
        {
            var key = $"Progress_{gameId}";
            return await LoadAsync(key, new PlayerProgressData(gameId));
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Get the full key name with prefix.
        /// </summary>
        private string GetFullKey(string key)
        {
            return $"{KEY_PREFIX}{key}";
        }
        
        /// <summary>
        /// Save the list of known keys to PlayerPrefs.
        /// </summary>
        private void SaveKnownKeys()
        {
            var keysArray = new string[knownKeys.Count];
            knownKeys.CopyTo(keysArray);
            var keysJson = JsonUtility.ToJson(new { keys = keysArray });
            PlayerPrefs.SetString($"{KEY_PREFIX}KnownKeys", keysJson);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load the list of known keys from PlayerPrefs.
        /// </summary>
        private void LoadKnownKeys()
        {
            try
            {
                var keysKey = $"{KEY_PREFIX}KnownKeys";
                if (PlayerPrefs.HasKey(keysKey))
                {
                    var keysJson = PlayerPrefs.GetString(keysKey);
                    var keysData = JsonUtility.FromJson<KeysData>(keysJson);
                    
                    if (keysData?.keys != null)
                    {
                        knownKeys.Clear();
                        foreach (var key in keysData.keys)
                        {
                            knownKeys.Add(key);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Failed to load known keys: {e.Message}");
                knownKeys.Clear();
            }
        }
        
        #endregion
        
        #region Helper Classes
        
        [Serializable]
        private class KeysData
        {
            public string[] keys;
        }
        
        #endregion
    }
}