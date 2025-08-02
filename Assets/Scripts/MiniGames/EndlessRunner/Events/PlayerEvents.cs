using UnityEngine;
using Core.Events;

namespace EndlessRunner.Events
{
    #region Movement Events
    public class PlayerMovementEvent : GameEvent
    {
        public Vector3 Position { get; private set; }
        public Vector3 MovementDelta { get; private set; }
        public float Speed { get; private set; }
        public float DistanceTraveled { get; private set; }
        
        public PlayerMovementEvent(Vector3 position, Vector3 movementDelta, float speed, float distanceTraveled)
        {
            Position = position;
            MovementDelta = movementDelta;
            Speed = speed;
            DistanceTraveled = distanceTraveled;
        }
    }
    
    public class PlayerLateralMovementEvent : GameEvent
    {
        public float LateralDirection { get; private set; } // -1 for left, 0 for center, 1 for right
        public Vector3 NewPosition { get; private set; }
        public float MovementSpeed { get; private set; }
        
        public PlayerLateralMovementEvent(float lateralDirection, Vector3 newPosition, float movementSpeed)
        {
            LateralDirection = lateralDirection;
            NewPosition = newPosition;
            MovementSpeed = movementSpeed;
        }
    }
    
    public class PlayerSpeedChangedEvent : GameEvent
    {
        public float NewSpeed { get; private set; }
        public float PreviousSpeed { get; private set; }
        public float SpeedMultiplier { get; private set; }
        
        public PlayerSpeedChangedEvent(float newSpeed, float previousSpeed, float speedMultiplier)
        {
            NewSpeed = newSpeed;
            PreviousSpeed = previousSpeed;
            SpeedMultiplier = speedMultiplier;
        }
    }
    #endregion
    
    #region Action Events
    public class PlayerJumpEvent : GameEvent
    {
        public Vector3 JumpPosition { get; private set; }
        public float JumpForce { get; private set; }
        public float JumpHeight { get; private set; }
        public bool IsDoubleJump { get; private set; }
        
        public PlayerJumpEvent(Vector3 jumpPosition, float jumpForce, float jumpHeight, bool isDoubleJump = false)
        {
            JumpPosition = jumpPosition;
            JumpForce = jumpForce;
            JumpHeight = jumpHeight;
            IsDoubleJump = isDoubleJump;
        }
    }
    

    
    public class PlayerDashEvent : GameEvent
    {
        public Vector3 DashDirection { get; private set; }
        public float DashSpeed { get; private set; }
        public float DashDuration { get; private set; }
        
        public PlayerDashEvent(Vector3 dashDirection, float dashSpeed, float dashDuration)
        {
            DashDirection = dashDirection;
            DashSpeed = dashSpeed;
            DashDuration = dashDuration;
        }
    }
    #endregion
    
    #region State Events
    public class PlayerStateChangedEvent : GameEvent
    {
        public string NewState { get; private set; }
        public string PreviousState { get; private set; }
        public float StateChangeTime { get; private set; }
        
        public PlayerStateChangedEvent(string newState, string previousState, float stateChangeTime)
        {
            NewState = newState;
            PreviousState = previousState;
            StateChangeTime = stateChangeTime;
        }
    }
    
    public class PlayerGroundedEvent : GameEvent
    {
        public bool IsGrounded { get; private set; }
        public Vector3 GroundPosition { get; private set; }
        public float GroundDistance { get; private set; }
        
        public PlayerGroundedEvent(bool isGrounded, Vector3 groundPosition, float groundDistance)
        {
            IsGrounded = isGrounded;
            GroundPosition = groundPosition;
            GroundDistance = groundDistance;
        }
    }
    
    public class PlayerInvincibilityEvent : GameEvent
    {
        public bool IsInvincible { get; private set; }
        public float InvincibilityDuration { get; private set; }
        public float RemainingTime { get; private set; }
        
        public PlayerInvincibilityEvent(bool isInvincible, float invincibilityDuration, float remainingTime)
        {
            IsInvincible = isInvincible;
            InvincibilityDuration = invincibilityDuration;
            RemainingTime = remainingTime;
        }
    }
    #endregion
    
    #region Health Events
    public class PlayerHealthChangedEvent : GameEvent
    {
        public int NewHealth { get; private set; }
        public int PreviousHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int HealthChange { get; private set; }
        
        public PlayerHealthChangedEvent(int newHealth, int previousHealth, int maxHealth, int healthChange)
        {
            NewHealth = newHealth;
            PreviousHealth = previousHealth;
            MaxHealth = maxHealth;
            HealthChange = healthChange;
        }
    }
    
    public class PlayerDamagedEvent : GameEvent
    {
        public int DamageAmount { get; private set; }
        public string DamageSource { get; private set; }
        public Vector3 DamagePosition { get; private set; }
        public bool IsCritical { get; private set; }
        
        public PlayerDamagedEvent(int damageAmount, string damageSource, Vector3 damagePosition, bool isCritical = false)
        {
            DamageAmount = damageAmount;
            DamageSource = damageSource;
            DamagePosition = damagePosition;
            IsCritical = isCritical;
        }
    }
    
    public class PlayerHealedEvent : GameEvent
    {
        public int HealAmount { get; private set; }
        public string HealSource { get; private set; }
        public bool IsFullHeal { get; private set; }
        
        public PlayerHealedEvent(int healAmount, string healSource, bool isFullHeal = false)
        {
            HealAmount = healAmount;
            HealSource = healSource;
            IsFullHeal = isFullHeal;
        }
    }
    #endregion
    
    #region Power-up Events
    public class PlayerPowerUpActivatedEvent : GameEvent
    {
        public string PowerUpType { get; private set; }
        public float Duration { get; private set; }
        public float EffectStrength { get; private set; }
        
        public PlayerPowerUpActivatedEvent(string powerUpType, float duration, float effectStrength)
        {
            PowerUpType = powerUpType;
            Duration = duration;
            EffectStrength = effectStrength;
        }
    }
    
    public class PlayerPowerUpDeactivatedEvent : GameEvent
    {
        public string PowerUpType { get; private set; }
        public float TotalActiveTime { get; private set; }
        
        public PlayerPowerUpDeactivatedEvent(string powerUpType, float totalActiveTime)
        {
            PowerUpType = powerUpType;
            TotalActiveTime = totalActiveTime;
        }
    }
    
    public class PlayerDeathEvent : GameEvent
    {
        public Vector3 DeathPosition { get; private set; }
        public string DeathCause { get; private set; }
        public int FinalScore { get; private set; }
        public float GameTime { get; private set; }
        
        public PlayerDeathEvent(Vector3 deathPosition, string deathCause, int finalScore, float gameTime)
        {
            DeathPosition = deathPosition;
            DeathCause = deathCause;
            FinalScore = finalScore;
            GameTime = gameTime;
        }
    }
    #endregion
} 