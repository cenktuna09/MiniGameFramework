namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// Types of obstacles in the Endless Runner game
    /// </summary>
    public enum ObstacleType
    {
        Block,      // Basic blocking obstacle
        Spike,      // Sharp obstacle that causes damage
        Wall,       // Tall wall obstacle
        Pit,        // Gap that player must jump over
        Moving,     // Moving obstacle
        Rotating    // Rotating obstacle
    }
} 