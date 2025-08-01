using UnityEngine;
using Core.Events;

namespace EndlessRunner.Events
{
    #region World Generation Events
    public class WorldChunkGeneratedEvent : GameEvent
    {
        public int ChunkIndex { get; private set; }
        public Vector3 ChunkPosition { get; private set; }
        public float ChunkLength { get; private set; }
        public int ObstacleCount { get; private set; }
        public int CollectibleCount { get; private set; }
        
        public WorldChunkGeneratedEvent(int chunkIndex, Vector3 chunkPosition, float chunkLength, int obstacleCount, int collectibleCount)
        {
            ChunkIndex = chunkIndex;
            ChunkPosition = chunkPosition;
            ChunkLength = chunkLength;
            ObstacleCount = obstacleCount;
            CollectibleCount = collectibleCount;
        }
    }
    
    public class WorldChunkDespawnedEvent : GameEvent
    {
        public int ChunkIndex { get; private set; }
        public Vector3 ChunkPosition { get; private set; }
        public float DistanceFromPlayer { get; private set; }
        
        public WorldChunkDespawnedEvent(int chunkIndex, Vector3 chunkPosition, float distanceFromPlayer)
        {
            ChunkIndex = chunkIndex;
            ChunkPosition = chunkPosition;
            DistanceFromPlayer = distanceFromPlayer;
        }
    }
    
    public class WorldDifficultyChangedEvent : GameEvent
    {
        public float NewDifficulty { get; private set; }
        public float PreviousDifficulty { get; private set; }
        public float DistanceTraveled { get; private set; }
        public string DifficultyReason { get; private set; }
        
        public WorldDifficultyChangedEvent(float newDifficulty, float previousDifficulty, float distanceTraveled, string difficultyReason)
        {
            NewDifficulty = newDifficulty;
            PreviousDifficulty = previousDifficulty;
            DistanceTraveled = distanceTraveled;
            DifficultyReason = difficultyReason;
        }
    }
    #endregion
    
    #region Obstacle Events
    public class ObstacleSpawnedEvent : GameEvent
    {
        public GameObject Obstacle { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public string ObstacleType { get; private set; }
        public float ObstacleSpeed { get; private set; }
        public int LaneIndex { get; private set; }
        
        public ObstacleSpawnedEvent(GameObject obstacle, Vector3 spawnPosition, string obstacleType, float obstacleSpeed, int laneIndex)
        {
            Obstacle = obstacle;
            SpawnPosition = spawnPosition;
            ObstacleType = obstacleType;
            ObstacleSpeed = obstacleSpeed;
            LaneIndex = laneIndex;
        }
    }
    
    public class ObstacleDespawnedEvent : GameEvent
    {
        public GameObject Obstacle { get; private set; }
        public Vector3 DespawnPosition { get; private set; }
        public string ObstacleType { get; private set; }
        public float DistanceFromPlayer { get; private set; }
        
        public ObstacleDespawnedEvent(GameObject obstacle, Vector3 despawnPosition, string obstacleType, float distanceFromPlayer)
        {
            Obstacle = obstacle;
            DespawnPosition = despawnPosition;
            ObstacleType = obstacleType;
            DistanceFromPlayer = distanceFromPlayer;
        }
    }
    
    public class ObstacleCollisionEvent : GameEvent
    {
        public GameObject Obstacle { get; private set; }
        public GameObject Player { get; private set; }
        public Vector3 CollisionPoint { get; private set; }
        public string ObstacleType { get; private set; }
        public float CollisionForce { get; private set; }
        
        public ObstacleCollisionEvent(GameObject obstacle, GameObject player, Vector3 collisionPoint, string obstacleType, float collisionForce)
        {
            Obstacle = obstacle;
            Player = player;
            CollisionPoint = collisionPoint;
            ObstacleType = obstacleType;
            CollisionForce = collisionForce;
        }
    }
    
    public class ObstacleAvoidedEvent : GameEvent
    {
        public GameObject Obstacle { get; private set; }
        public Vector3 AvoidPosition { get; private set; }
        public string ObstacleType { get; private set; }
        public string AvoidMethod { get; private set; } // "Jump", "Slide", "Lateral"
        
        public ObstacleAvoidedEvent(GameObject obstacle, Vector3 avoidPosition, string obstacleType, string avoidMethod)
        {
            Obstacle = obstacle;
            AvoidPosition = avoidPosition;
            ObstacleType = obstacleType;
            AvoidMethod = avoidMethod;
        }
    }
    #endregion
    
    #region Collectible Events
    public class CollectibleSpawnedEvent : GameEvent
    {
        public GameObject Collectible { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public string CollectibleType { get; private set; }
        public int CollectibleValue { get; private set; }
        public int LaneIndex { get; private set; }
        
        public CollectibleSpawnedEvent(GameObject collectible, Vector3 spawnPosition, string collectibleType, int collectibleValue, int laneIndex)
        {
            Collectible = collectible;
            SpawnPosition = spawnPosition;
            CollectibleType = collectibleType;
            CollectibleValue = collectibleValue;
            LaneIndex = laneIndex;
        }
    }
    
    public class CollectiblePickedUpEvent : GameEvent
    {
        public GameObject Collectible { get; private set; }
        public GameObject Player { get; private set; }
        public Vector3 PickupPosition { get; private set; }
        public string CollectibleType { get; private set; }
        public int CollectibleValue { get; private set; }
        public bool IsCombo { get; private set; }
        
        public CollectiblePickedUpEvent(GameObject collectible, GameObject player, Vector3 pickupPosition, string collectibleType, int collectibleValue, bool isCombo = false)
        {
            Collectible = collectible;
            Player = player;
            PickupPosition = pickupPosition;
            CollectibleType = collectibleType;
            CollectibleValue = collectibleValue;
            IsCombo = isCombo;
        }
    }
    
    public class CollectibleCollectedEvent : GameEvent
    {
        public GameObject Collectible { get; private set; }
        public GameObject Player { get; private set; }
        public Vector3 Position { get; private set; }
        public string CollectibleType { get; private set; }
        public int PointValue { get; private set; }
        public int Lane { get; private set; }
        
        public CollectibleCollectedEvent(GameObject collectible, GameObject player, Vector3 position, string collectibleType, int pointValue, int lane)
        {
            Collectible = collectible;
            Player = player;
            Position = position;
            CollectibleType = collectibleType;
            PointValue = pointValue;
            Lane = lane;
        }
    }
    
    public class CollectibleMissedEvent : GameEvent
    {
        public GameObject Collectible { get; private set; }
        public Vector3 MissPosition { get; private set; }
        public string CollectibleType { get; private set; }
        public int CollectibleValue { get; private set; }
        public float DistanceFromPlayer { get; private set; }
        
        public CollectibleMissedEvent(GameObject collectible, Vector3 missPosition, string collectibleType, int collectibleValue, float distanceFromPlayer)
        {
            Collectible = collectible;
            MissPosition = missPosition;
            CollectibleType = collectibleType;
            CollectibleValue = collectibleValue;
            DistanceFromPlayer = distanceFromPlayer;
        }
    }
    
    public class CollectibleComboEvent : GameEvent
    {
        public int ComboCount { get; private set; }
        public int TotalComboValue { get; private set; }
        public float ComboTimeWindow { get; private set; }
        public string ComboType { get; private set; }
        
        public CollectibleComboEvent(int comboCount, int totalComboValue, float comboTimeWindow, string comboType)
        {
            ComboCount = comboCount;
            TotalComboValue = totalComboValue;
            ComboTimeWindow = comboTimeWindow;
            ComboType = comboType;
        }
    }
    #endregion
    
    #region Environment Events
    public class EnvironmentChangedEvent : GameEvent
    {
        public string NewEnvironment { get; private set; }
        public string PreviousEnvironment { get; private set; }
        public Vector3 ChangePosition { get; private set; }
        public float TransitionDuration { get; private set; }
        
        public EnvironmentChangedEvent(string newEnvironment, string previousEnvironment, Vector3 changePosition, float transitionDuration)
        {
            NewEnvironment = newEnvironment;
            PreviousEnvironment = previousEnvironment;
            ChangePosition = changePosition;
            TransitionDuration = transitionDuration;
        }
    }
    
    public class WeatherChangedEvent : GameEvent
    {
        public string NewWeather { get; private set; }
        public string PreviousWeather { get; private set; }
        public float WeatherIntensity { get; private set; }
        public float WeatherDuration { get; private set; }
        
        public WeatherChangedEvent(string newWeather, string previousWeather, float weatherIntensity, float weatherDuration)
        {
            NewWeather = newWeather;
            PreviousWeather = previousWeather;
            WeatherIntensity = weatherIntensity;
            WeatherDuration = weatherDuration;
        }
    }
    
    public class TimeOfDayChangedEvent : GameEvent
    {
        public float NewTimeOfDay { get; private set; } // 0-24 hours
        public float PreviousTimeOfDay { get; private set; }
        public string TimePeriod { get; private set; } // "Morning", "Afternoon", "Evening", "Night"
        
        public TimeOfDayChangedEvent(float newTimeOfDay, float previousTimeOfDay, string timePeriod)
        {
            NewTimeOfDay = newTimeOfDay;
            PreviousTimeOfDay = previousTimeOfDay;
            TimePeriod = timePeriod;
        }
    }
    #endregion
    
    #region Lane Events
    public class LaneChangedEvent : GameEvent
    {
        public int NewLane { get; private set; }
        public int PreviousLane { get; private set; }
        public Vector3 LanePosition { get; private set; }
        public float LaneChangeSpeed { get; private set; }
        
        public LaneChangedEvent(int newLane, int previousLane, Vector3 lanePosition, float laneChangeSpeed)
        {
            NewLane = newLane;
            PreviousLane = previousLane;
            LanePosition = lanePosition;
            LaneChangeSpeed = laneChangeSpeed;
        }
    }
    
    public class LaneBlockedEvent : GameEvent
    {
        public int LaneIndex { get; private set; }
        public Vector3 BlockPosition { get; private set; }
        public string BlockReason { get; private set; }
        public float BlockDuration { get; private set; }
        
        public LaneBlockedEvent(int laneIndex, Vector3 blockPosition, string blockReason, float blockDuration)
        {
            LaneIndex = laneIndex;
            BlockPosition = blockPosition;
            BlockReason = blockReason;
            BlockDuration = blockDuration;
        }
    }
    #endregion
} 