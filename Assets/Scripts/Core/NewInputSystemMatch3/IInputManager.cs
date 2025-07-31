using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniGameFramework.Core.Input
{
    /// <summary>
    /// Interface for the input management system.
    /// Provides clean abstraction for input handling across different game contexts.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Initialize the input system with specified context.
        /// </summary>
        /// <param name="context">The input context to activate</param>
        void Initialize(InputContext context = InputContext.Menu);

        /// <summary>
        /// Switch to a different input context.
        /// </summary>
        /// <param name="context">The new input context</param>
        void SwitchContext(InputContext context);

        /// <summary>
        /// Enable input processing.
        /// </summary>
        void EnableInput();

        /// <summary>
        /// Disable input processing.
        /// </summary>
        void DisableInput();

        /// <summary>
        /// Check if input is currently enabled.
        /// </summary>
        bool IsInputEnabled { get; }

        /// <summary>
        /// Get current input context.
        /// </summary>
        InputContext CurrentContext { get; }

        /// <summary>
        /// Enable/disable specific input map.
        /// </summary>
        /// <param name="mapName">Name of the input action map</param>
        /// <param name="enabled">Enable or disable</param>
        void SetInputMapEnabled(string mapName, bool enabled);

        /// <summary>
        /// Get current pointer/mouse position.
        /// </summary>
        Vector2 GetPointerPosition();

        /// <summary>
        /// Check if a specific action is currently pressed.
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        bool IsActionPressed(string actionName);

        /// <summary>
        /// Get the current value of an action (for axes/vector inputs).
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        float GetActionValue(string actionName);

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// Input contexts for different game states.
    /// </summary>
    public enum InputContext
    {
        Menu,
        Match3,
        EndlessRunner,
        Paused
    }
}