namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Defines the different terrain types available on the tactical battlefield.
/// Each terrain type affects movement, combat, and provides different tactical options.
/// </summary>
public enum TerrainType
{
    /// <summary>
    /// Standard terrain with no special effects.
    /// Movement cost: 1, no combat modifiers.
    /// </summary>
    Plains,

    /// <summary>
    /// Wooded terrain that provides cover but slows movement.
    /// Movement cost: 2, +1 Defense when defending.
    /// </summary>
    Forest,

    /// <summary>
    /// Elevated terrain that provides combat advantages.
    /// Movement cost: 1, +1 Attack when attacking from high ground.
    /// </summary>
    Hill,

    /// <summary>
    /// Dangerous terrain that damages units standing on or pushed into it.
    /// Movement cost: 1, deals 2 damage at end of turn if unit is on tile.
    /// </summary>
    Hazard,

    /// <summary>
    /// Impassable terrain - units cannot enter or occupy.
    /// Used for shaping the battlefield.
    /// </summary>
    Water,

    /// <summary>
    /// Difficult terrain that slows movement significantly.
    /// Movement cost: 2, reduces push/pull effectiveness.
    /// </summary>
    Mud,

    /// <summary>
    /// Slippery terrain that extends movement and push effects.
    /// Movement cost: 1, pushed units slide an extra tile.
    /// </summary>
    Ice,

    /// <summary>
    /// Void/off-map - tile does not exist.
    /// Used to create non-rectangular battlefield shapes.
    /// </summary>
    Void
}

/// <summary>
/// Extension methods for TerrainType to get terrain properties.
/// </summary>
public static class TerrainTypeExtensions
{
    /// <summary>
    /// Gets the movement cost to enter this terrain type.
    /// </summary>
    public static int GetMovementCost(this TerrainType terrain) => terrain switch
    {
        TerrainType.Plains => 1,
        TerrainType.Forest => 2,
        TerrainType.Hill => 1,
        TerrainType.Hazard => 1,
        TerrainType.Water => 99, // Impassable
        TerrainType.Mud => 2,
        TerrainType.Ice => 1,
        TerrainType.Void => 99, // Impassable
        _ => 1
    };

    /// <summary>
    /// Returns true if units can enter this terrain.
    /// </summary>
    public static bool IsPassable(this TerrainType terrain) => terrain switch
    {
        TerrainType.Water => false,
        TerrainType.Void => false,
        _ => true
    };

    /// <summary>
    /// Gets the defense bonus provided by this terrain.
    /// </summary>
    public static int GetDefenseBonus(this TerrainType terrain) => terrain switch
    {
        TerrainType.Forest => 1,
        _ => 0
    };

    /// <summary>
    /// Gets the attack bonus provided by this terrain.
    /// </summary>
    public static int GetAttackBonus(this TerrainType terrain) => terrain switch
    {
        TerrainType.Hill => 1,
        _ => 0
    };

    /// <summary>
    /// Gets the hazard damage dealt to units on this terrain at end of turn.
    /// </summary>
    public static int GetHazardDamage(this TerrainType terrain) => terrain switch
    {
        TerrainType.Hazard => 2,
        _ => 0
    };

    /// <summary>
    /// Returns true if this terrain causes extra slide when pushed.
    /// </summary>
    public static bool CausesSlide(this TerrainType terrain) => terrain == TerrainType.Ice;

    /// <summary>
    /// Returns a display name for the terrain type.
    /// </summary>
    public static string GetDisplayName(this TerrainType terrain) => terrain switch
    {
        TerrainType.Plains => "Plains",
        TerrainType.Forest => "Forest (+1 Def)",
        TerrainType.Hill => "Hill (+1 Atk)",
        TerrainType.Hazard => "Hazard (2 dmg/turn)",
        TerrainType.Water => "Water",
        TerrainType.Mud => "Mud (slow)",
        TerrainType.Ice => "Ice (slippery)",
        TerrainType.Void => "",
        _ => ""
    };
}
