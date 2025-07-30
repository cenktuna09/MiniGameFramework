using UnityEngine;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.Core.Input
{
    /// <summary>
    /// Base class for all input-related events.
    /// </summary>
    public abstract class InputEvent : GameEvent
    {
        /// <summary>
        /// Timestamp when the input occurred.
        /// </summary>
        public new float Timestamp { get; }

        protected InputEvent()
        {
            Timestamp = Time.time;
        }
    }

    /// <summary>
    /// Event fired when player movement input is detected.
    /// </summary>
    public class PlayerMoveInputEvent : InputEvent
    {
        public Vector2 MoveDirection { get; }
        public bool IsPressed { get; }

        public PlayerMoveInputEvent(Vector2 moveDirection, bool isPressed)
        {
            MoveDirection = moveDirection;
            IsPressed = isPressed;
        }
    }

    /// <summary>
    /// Event fired when player look input is detected.
    /// </summary>
    public class PlayerLookInputEvent : InputEvent
    {
        public Vector2 LookDelta { get; }

        public PlayerLookInputEvent(Vector2 lookDelta)
        {
            LookDelta = lookDelta;
        }
    }

    /// <summary>
    /// Event fired when player action button is pressed/released.
    /// </summary>
    public class PlayerActionInputEvent : InputEvent
    {
        public enum ActionType
        {
            Attack,
            Jump,
            Interact,
            Crouch,
            Sprint,
            Previous,
            Next
        }

        public ActionType Action { get; }
        public bool IsPressed { get; }
        public bool IsHeld { get; }

        public PlayerActionInputEvent(ActionType action, bool isPressed, bool isHeld = false)
        {
            Action = action;
            IsPressed = isPressed;
            IsHeld = isHeld;
        }
    }

    /// <summary>
    /// Event fired when UI navigation input is detected.
    /// </summary>
    public class UINavigationInputEvent : InputEvent
    {
        public Vector2 NavigationDirection { get; }

        public UINavigationInputEvent(Vector2 navigationDirection)
        {
            NavigationDirection = navigationDirection;
        }
    }

    /// <summary>
    /// Event fired when UI action button is pressed.
    /// </summary>
    public class UIActionInputEvent : InputEvent
    {
        public enum UIActionType
        {
            Submit,
            Cancel,
            Click,
            RightClick,
            MiddleClick
        }

        public UIActionType Action { get; }
        public Vector2 PointerPosition { get; }

        public UIActionInputEvent(UIActionType action, Vector2 pointerPosition = default)
        {
            Action = action;
            PointerPosition = pointerPosition;
        }
    }

    /// <summary>
    /// Event fired when scroll wheel input is detected.
    /// </summary>
    public class ScrollWheelInputEvent : InputEvent
    {
        public Vector2 ScrollDelta { get; }

        public ScrollWheelInputEvent(Vector2 scrollDelta)
        {
            ScrollDelta = scrollDelta;
        }
    }

    // === Match3 Specific Events ===

    /// <summary>
    /// Event fired when a tile is clicked/tapped in Match3 game.
    /// </summary>
    public class TileClickInputEvent : InputEvent
    {
        public Vector2 ScreenPosition { get; }
        public Vector2 WorldPosition { get; }

        public TileClickInputEvent(Vector2 screenPosition, Vector2 worldPosition)
        {
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
        }
    }

    /// <summary>
    /// Event fired when a swipe gesture is detected for Match3 game.
    /// </summary>
    public class SwipeInputEvent : InputEvent
    {
        public Vector2 StartPosition { get; }
        public Vector2 EndPosition { get; }
        public Vector2 Direction { get; }
        public float Distance { get; }

        public SwipeInputEvent(Vector2 startPosition, Vector2 endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Direction = (endPosition - startPosition).normalized;
            Distance = Vector2.Distance(startPosition, endPosition);
        }
    }

    // === Endless Runner Specific Events ===

    /// <summary>
    /// Event fired when player wants to change lanes in Endless Runner.
    /// </summary>
    public class LaneChangeInputEvent : InputEvent
    {
        public enum LaneDirection
        {
            Left = -1,
            Right = 1
        }

        public LaneDirection Direction { get; }

        public LaneChangeInputEvent(LaneDirection direction)
        {
            Direction = direction;
        }
    }

    /// <summary>
    /// Event fired when player performs slide action in Endless Runner.
    /// </summary>
    public class SlideInputEvent : InputEvent
    {
        public bool IsStarting { get; }

        public SlideInputEvent(bool isStarting = true)
        {
            IsStarting = isStarting;
        }
    }
}