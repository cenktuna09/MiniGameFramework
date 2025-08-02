using UnityEngine;
using UnityEngine.UI;
using Core.Scene;

namespace Core.UI
{
    /// <summary>
    /// Simple pause menu UI component
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Pause Menu UI")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Text pauseText;
        
        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextGameButton;

        private GameMenu _gameMenu;

        void Start()
        {
            SetupButtons();
            
            // Get GameMenu reference
            _gameMenu = FindFirstObjectByType<GameMenu>();
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                
            if (nextGameButton != null)
                nextGameButton.onClick.AddListener(OnNextGameClicked);
        }

        public void ShowPauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void HidePauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        #region Button Event Handlers

        private void OnResumeClicked()
        {
            if (_gameMenu != null)
            {
                _gameMenu.ResumeGame();
            }
        }

        private void OnRestartClicked()
        {
            if (_gameMenu != null)
            {
                _gameMenu.RestartGame();
            }
        }

        private void OnMainMenuClicked()
        {
            if (_gameMenu != null)
            {
                _gameMenu.GoToMainMenu();
            }
        }

        private void OnNextGameClicked()
        {
            if (_gameMenu != null)
            {
                _gameMenu.GoToNextGame();
            }
        }

        #endregion
    }
}