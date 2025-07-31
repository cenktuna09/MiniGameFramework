namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Constants for scene names and indices.
    /// Provides a centralized way to reference scenes without magic strings.
    /// </summary>
    public static class SceneConstants
    {
        // Core scenes
        public const string MAIN_MENU = "MainMenu";
        public const string SAMPLE_SCENE = "SampleScene";
        public const string SCENE_MANAGEMENT_TEST = "SceneManagementTest";
        
        // MiniGame scenes
        public const string MATCH3_GAME = "Match3Game";
        public const string ENDLESS_RUNNER = "EndlessRunner";
        
        // Build indices (should match build settings)
        public static class BuildIndex
        {
            public const int MAIN_MENU = 0;
            public const int SAMPLE_SCENE = 1;
            public const int SCENE_MANAGEMENT_TEST = 2;
            public const int MATCH3_GAME = 3;
            public const int ENDLESS_RUNNER = 4;
        }
        
        /// <summary>
        /// Get all available scene names.
        /// </summary>
        public static string[] GetAllSceneNames()
        {
            return new string[]
            {
                MAIN_MENU,
                SAMPLE_SCENE,
                SCENE_MANAGEMENT_TEST,
                MATCH3_GAME,
                ENDLESS_RUNNER
            };
        }
        
        /// <summary>
        /// Check if a scene name is valid.
        /// </summary>
        /// <param name="sceneName">Scene name to validate</param>
        /// <returns>True if scene name exists in constants</returns>
        public static bool IsValidSceneName(string sceneName)
        {
            var allScenes = GetAllSceneNames();
            for (int i = 0; i < allScenes.Length; i++)
            {
                if (allScenes[i] == sceneName)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get scene name by build index.
        /// </summary>
        /// <param name="buildIndex">Build index</param>
        /// <returns>Scene name or null if invalid index</returns>
        public static string GetSceneNameByIndex(int buildIndex)
        {
            switch (buildIndex)
            {
                case BuildIndex.MAIN_MENU: return MAIN_MENU;
                case BuildIndex.SAMPLE_SCENE: return SAMPLE_SCENE;
                case BuildIndex.SCENE_MANAGEMENT_TEST: return SCENE_MANAGEMENT_TEST;
                case BuildIndex.MATCH3_GAME: return MATCH3_GAME;
                case BuildIndex.ENDLESS_RUNNER: return ENDLESS_RUNNER;
                default: return null;
            }
        }
    }
} 