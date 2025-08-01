using System;
using UnityEngine;
using Core.Events;
using Core.Architecture;

namespace EndlessRunner.Core.Interfaces
{
    /// <summary>
    /// Interface for player controller functionality
    /// Provides abstraction for player movement and state
    /// </summary>
    public interface IPlayerController
    {
        /// <summary>
        /// Current position of the player
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Whether the player is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Whether the player is currently dead
        /// </summary>
        bool IsDead { get; }
        
        /// <summary>
        /// Current health of the player
        /// </summary>
        int CurrentHealth { get; }
        
        /// <summary>
        /// Maximum health of the player
        /// </summary>
        int MaxHealth { get; }
        
        /// <summary>
        /// Make the player jump
        /// </summary>
        void Jump();
        
        /// <summary>
        /// Make the player slide
        /// </summary>
        void Slide();
        
        /// <summary>
        /// Move the player horizontally
        /// </summary>
        /// <param name="direction">Direction to move (-1 for left, 1 for right)</param>
        void MoveHorizontal(float direction);
        
        /// <summary>
        /// Take damage
        /// </summary>
        /// <param name="damageAmount">Amount of damage to take</param>
        void TakeDamage(int damageAmount);
        
        /// <summary>
        /// Reset the player to initial state
        /// </summary>
        void ResetPlayer();
        
        /// <summary>
        /// Initialize the player controller
        /// </summary>
        /// <param name="eventBus">Event bus for communication</param>
        void Initialize(IEventBus eventBus);
        
        /// <summary>
        /// Event fired when player position changes
        /// </summary>
        event Action<Vector3> OnPositionChanged;
        
        /// <summary>
        /// Event fired when player health changes
        /// </summary>
        event Action<int, int> OnHealthChanged;
        
        /// <summary>
        /// Event fired when player dies
        /// </summary>
        event Action<string> OnPlayerDeath;
    }
} 