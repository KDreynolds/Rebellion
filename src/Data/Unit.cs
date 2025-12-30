using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Represents a tactical unit on the battlefield.
/// Units have grid position, combat stats, and visual properties.
/// </summary>
public class Unit
{
    /// <summary>
    /// Current position on the grid (column, row).
    /// </summary>
    public Point GridPosition { get; set; }

    /// <summary>
    /// Display name for the unit.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Color used to render this unit.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Whether this unit belongs to the player's team.
    /// </summary>
    public bool IsPlayerUnit { get; set; }

    // ============================================
    // HERO STATS
    // ============================================

    /// <summary>
    /// Maximum hit points for this unit.
    /// </summary>
    public int MaxHP { get; set; }

    /// <summary>
    /// Current hit points. Unit is defeated when this reaches 0.
    /// </summary>
    public int CurrentHP { get; set; }

    /// <summary>
    /// Maximum tiles this unit can move in a single turn (Manhattan distance).
    /// </summary>
    public int MoveRange { get; set; }

    /// <summary>
    /// Range at which this unit can attack (Manhattan distance).
    /// 1 = melee, 2+ = ranged.
    /// </summary>
    public int AttackRange { get; set; }

    /// <summary>
    /// Base damage dealt by this unit's basic attack.
    /// </summary>
    public int AttackPower { get; set; }

    /// <summary>
    /// Reduces incoming damage. Damage taken = AttackPower - Defense (minimum 1).
    /// </summary>
    public int Defense { get; set; }

    /// <summary>
    /// Resistance to push/pull effects. Higher values reduce displacement.
    /// 0 = normal, 1 = reduced by 1 tile, 2+ = immovable.
    /// </summary>
    public int PushResistance { get; set; }

    /// <summary>
    /// House affiliation for lore and realm buff purposes.
    /// </summary>
    public string? HouseAffiliation { get; set; }

    /// <summary>
    /// The passive trait this hero possesses.
    /// </summary>
    public PassiveTrait? PassiveTrait { get; set; }

    /// <summary>
    /// List of abilities this unit can use.
    /// </summary>
    public List<Ability> Abilities { get; set; } = new();

    /// <summary>
    /// Whether this unit is still alive.
    /// </summary>
    public bool IsAlive => CurrentHP > 0;

    /// <summary>
    /// Current HP as a percentage of max HP (0.0 to 1.0).
    /// </summary>
    public float HPPercentage => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    public Unit(string name, Point gridPosition, int moveRange, Color color, bool isPlayerUnit = true)
        : this(name, gridPosition, moveRange, color, maxHP: 10, attackRange: 1, attackPower: 3, defense: 0, isPlayerUnit)
    {
    }

    public Unit(
        string name,
        Point gridPosition,
        int moveRange,
        Color color,
        int maxHP,
        int attackRange,
        int attackPower,
        int defense,
        bool isPlayerUnit = true,
        int pushResistance = 0,
        string? houseAffiliation = null,
        PassiveTrait? passiveTrait = null)
    {
        Name = name;
        GridPosition = gridPosition;
        MoveRange = moveRange;
        Color = color;
        MaxHP = maxHP;
        CurrentHP = maxHP;
        AttackRange = attackRange;
        AttackPower = attackPower;
        Defense = defense;
        IsPlayerUnit = isPlayerUnit;
        PushResistance = pushResistance;
        HouseAffiliation = houseAffiliation;
        PassiveTrait = passiveTrait;
    }

    /// <summary>
    /// Calculates the Manhattan distance from this unit to a target tile.
    /// </summary>
    public int DistanceTo(Point target)
    {
        return Math.Abs(GridPosition.X - target.X) + Math.Abs(GridPosition.Y - target.Y);
    }

    /// <summary>
    /// Checks if a target tile is within this unit's movement range.
    /// </summary>
    public bool CanReach(Point target)
    {
        return DistanceTo(target) <= MoveRange;
    }

    /// <summary>
    /// Checks if a target tile is within this unit's attack range.
    /// </summary>
    public bool CanAttack(Point target)
    {
        return DistanceTo(target) <= AttackRange && DistanceTo(target) > 0;
    }

    /// <summary>
    /// Applies damage to this unit, accounting for defense.
    /// </summary>
    /// <param name="incomingDamage">Raw damage before defense calculation.</param>
    /// <returns>Actual damage dealt after defense.</returns>
    public int TakeDamage(int incomingDamage)
    {
        int actualDamage = Math.Max(1, incomingDamage - Defense);
        CurrentHP = Math.Max(0, CurrentHP - actualDamage);
        return actualDamage;
    }

    /// <summary>
    /// Heals this unit by the specified amount, not exceeding MaxHP.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    /// <returns>Actual amount healed.</returns>
    public int Heal(int amount)
    {
        int previousHP = CurrentHP;
        CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
        return CurrentHP - previousHP;
    }

    /// <summary>
    /// Calculates push displacement after accounting for push resistance.
    /// </summary>
    /// <param name="pushDistance">Intended push distance.</param>
    /// <returns>Actual displacement distance.</returns>
    public int CalculatePushDisplacement(int pushDistance)
    {
        return Math.Max(0, pushDistance - PushResistance);
    }
}
