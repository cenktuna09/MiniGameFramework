using UnityEngine;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Configuration data for scene transitions.
    /// </summary>
    [System.Serializable]
    public struct SceneTransitionData
    {
        [Header("Transition Settings")]
        public TransitionType transitionType;
        public float transitionDuration;
        public AnimationCurve transitionCurve;
        
        [Header("Fade Settings")]
        public Color fadeColor;
        
        [Header("Loading Settings")]
        public bool showLoadingScreen;
        public string loadingText;
        
        public static SceneTransitionData Default => new SceneTransitionData
        {
            transitionType = TransitionType.Fade,
            transitionDuration = 0.5f,
            transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
            fadeColor = Color.black,
            showLoadingScreen = true,
            loadingText = "Loading..."
        };
    }
    
    /// <summary>
    /// Types of scene transitions available.
    /// </summary>
    public enum TransitionType
    {
        None,
        Fade,
        Slide,
        Zoom,
        Custom
    }
    
    /// <summary>
    /// Event data for scene loading progress updates.
    /// </summary>
    public class SceneLoadingProgressEvent : Architecture.GameEvent
    {
        public string SceneName { get; }
        public float Progress { get; }
        public string LoadingText { get; }
        
        public SceneLoadingProgressEvent(string sceneName, float progress, string loadingText = null, object source = null) 
            : base(source)
        {
            SceneName = sceneName;
            Progress = Mathf.Clamp01(progress);
            LoadingText = loadingText ?? "Loading...";
        }
    }
    
    /// <summary>
    /// Event data for scene transition state changes.
    /// </summary>
    public class SceneTransitionEvent : Architecture.GameEvent
    {
        public TransitionState State { get; }
        public string SceneName { get; }
        public TransitionType TransitionType { get; }
        
        public SceneTransitionEvent(TransitionState state, string sceneName, TransitionType transitionType, object source = null) 
            : base(source)
        {
            State = state;
            SceneName = sceneName;
            TransitionType = transitionType;
        }
    }
    
    /// <summary>
    /// States of scene transition process.
    /// </summary>
    public enum TransitionState
    {
        Started,
        FadeOutStarted,
        FadeOutCompleted,
        LoadingStarted,
        LoadingProgress,
        LoadingCompleted,
        FadeInStarted,
        FadeInCompleted,
        Completed
    }
    
    /// <summary>
    /// Scene reference data for easy scene management.
    /// </summary>
    [System.Serializable]
    public struct SceneReference
    {
        public string sceneName;
        public int buildIndex;
        public bool isAddressable;
        public string addressableKey;
        
        public bool IsValid => !string.IsNullOrEmpty(sceneName) || buildIndex >= 0;
        
        public string GetLoadKey()
        {
            if (isAddressable && !string.IsNullOrEmpty(addressableKey))
                return addressableKey;
            return sceneName;
        }
        
        public static implicit operator string(SceneReference sceneRef) => sceneRef.GetLoadKey();
        public static implicit operator SceneReference(string sceneName) => new SceneReference { sceneName = sceneName };
    }
} 