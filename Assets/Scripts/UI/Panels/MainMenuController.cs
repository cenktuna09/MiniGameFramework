using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using MiniGameFramework.Core.Bootstrap;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Controls the main menu UI and handles game loading buttons
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _endlessRunnerButton;
        [SerializeField] private Button _match3Button;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        
        [Header("Loading Configuration")]
        [SerializeField] private GameObject _loadingScreenPrefab;
        [SerializeField] private bool _useLoadingScreen = true;
        [SerializeField] private bool _useAsyncLoading = true; // New option for async loading
        
        [Header("Audio")]
        [SerializeField] private AudioSource _buttonClickAudio;
        
        private void Start()
        {
            SetupButtons();
            Debug.Log("[MainMenuController] Main menu initialized");
        }
        
        private void SetupButtons()
        {
            // EndlessRunner Button
            if (_endlessRunnerButton != null)
            {
                _endlessRunnerButton.onClick.AddListener(LoadEndlessRunner);
                Debug.Log("[MainMenuController] EndlessRunner button configured");
            }
            
            // Match3 Button
            if (_match3Button != null)
            {
                _match3Button.onClick.AddListener(LoadMatch3);
                Debug.Log("[MainMenuController] Match3 button configured");
            }
            
            // Settings Button
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OpenSettings);
                Debug.Log("[MainMenuController] Settings button configured");
            }
            
            // Quit Button
            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(QuitGame);
                Debug.Log("[MainMenuController] Quit button configured");
            }
        }
        
        #region Button Event Handlers
        
        /// <summary>
        /// Load EndlessRunner game
        /// </summary>
        public void LoadEndlessRunner()
        {
            PlayButtonClickSound();
            Debug.Log("[MainMenuController] Loading EndlessRunner...");
            
            if (_useAsyncLoading)
            {
                _ = LoadEndlessRunnerAsync(); // Fire and forget async
            }
            else
            {
                if (_useLoadingScreen && _loadingScreenPrefab != null)
                {
                    MiniGameLoader.LoadGameWithLoadingScreen("EndlessRunner", _loadingScreenPrefab);
                }
                else
                {
                    MiniGameLoader.LoadGame("EndlessRunner");
                }
            }
        }
        
        /// <summary>
        /// Load Match3 game
        /// </summary>
        public void LoadMatch3()
        {
            PlayButtonClickSound();
            Debug.Log("[MainMenuController] Loading Match3...");
            
            if (_useAsyncLoading)
            {
                _ = LoadMatch3Async(); // Fire and forget async
            }
            else
            {
                if (_useLoadingScreen && _loadingScreenPrefab != null)
                {
                    MiniGameLoader.LoadGameWithLoadingScreen("Match3", _loadingScreenPrefab);
                }
                else
                {
                    MiniGameLoader.LoadGame("Match3");
                }
            }
        }
        
        /// <summary>
        /// Load EndlessRunner game asynchronously
        /// </summary>
        public async Task LoadEndlessRunnerAsync()
        {
            try
            {
                if (_useLoadingScreen && _loadingScreenPrefab != null)
                {
                    await MiniGameLoader.LoadGameWithLoadingScreenAsync("EndlessRunner", _loadingScreenPrefab);
                }
                else
                {
                    await MiniGameLoader.LoadGameAsync("EndlessRunner");
                }
                
                Debug.Log("[MainMenuController] EndlessRunner loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuController] Failed to load EndlessRunner: {ex.Message}");
                // You can show an error UI here
            }
        }
        
        /// <summary>
        /// Load Match3 game asynchronously
        /// </summary>
        public async Task LoadMatch3Async()
        {
            try
            {
                if (_useLoadingScreen && _loadingScreenPrefab != null)
                {
                    await MiniGameLoader.LoadGameWithLoadingScreenAsync("Match3", _loadingScreenPrefab);
                }
                else
                {
                    await MiniGameLoader.LoadGameAsync("Match3");
                }
                
                Debug.Log("[MainMenuController] Match3 loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainMenuController] Failed to load Match3: {ex.Message}");
                // You can show an error UI here
            }
        }
        
        /// <summary>
        /// Open settings menu
        /// </summary>
        public void OpenSettings()
        {
            PlayButtonClickSound();
            Debug.Log("[MainMenuController] Opening settings...");
            // TODO: Implement settings panel
        }
        
        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            PlayButtonClickSound();
            Debug.Log("[MainMenuController] Quitting game...");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        #endregion
        
        #region Utility Methods
        
        private void PlayButtonClickSound()
        {
            if (_buttonClickAudio != null)
            {
                _buttonClickAudio.Play();
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clean up button listeners
            if (_endlessRunnerButton != null)
                _endlessRunnerButton.onClick.RemoveListener(LoadEndlessRunner);
                
            if (_match3Button != null)
                _match3Button.onClick.RemoveListener(LoadMatch3);
                
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OpenSettings);
                
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(QuitGame);
        }
        
        #endregion
    }
} 