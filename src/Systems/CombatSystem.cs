using Microsoft.Xna.Framework;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Handles combat resolution between units.
/// Manages attacks, damage calculation, and combat effects.
/// </summary>
public class CombatSystem
{
    private readonly List<Unit> _units;

    /// <summary>
    /// Event fired when a unit takes damage.
    /// </summary>
    public event Action<Unit, int>? OnUnitDamaged;

    /// <summary>
    /// Event fired when a unit is defeated.
    /// </summary>
    public event Action<Unit>? OnUnitDefeated;

    /// <summary>
    /// Event fired when an attack is executed.
    /// </summary>
    public event Action<Unit, Unit, int>? OnAttackExecuted;

    public CombatSystem(List<Unit> units)
    {
        _units = units;
    }

    /// <summary>
    /// Checks if an attacker can attack a target.
    /// </summary>
    /// <param name="attacker">The attacking unit.</param>
    /// <param name="target">The potential target.</param>
    /// <returns>True if the attack is valid.</returns>
    public bool CanAttack(Unit attacker, Unit target)
    {
        if (attacker == null || target == null) return false;
        if (!attacker.IsAlive || !target.IsAlive) return false;
        if (attacker == target) return false;
        if (attacker.IsPlayerUnit == target.IsPlayerUnit) return false; // Can't attack allies

        return attacker.CanAttack(target.GridPosition);
    }

    /// <summary>
    /// Gets all valid attack targets for a unit.
    /// </summary>
    /// <param name="attacker">The attacking unit.</param>
    /// <returns>List of units that can be attacked.</returns>
    public List<Unit> GetValidTargets(Unit attacker)
    {
        var targets = new List<Unit>();

        if (attacker == null || !attacker.IsAlive) return targets;

        foreach (var unit in _units)
        {
            if (CanAttack(attacker, unit))
            {
                targets.Add(unit);
            }
        }

        return targets;
    }

    /// <summary>
    /// Gets all tiles within attack range of a unit that contain enemies.
    /// </summary>
    /// <param name="attacker">The attacking unit.</param>
    /// <returns>List of grid positions with attackable enemies.</returns>
    public List<Point> GetAttackableTiles(Unit attacker)
    {
        var tiles = new List<Point>();

        foreach (var target in GetValidTargets(attacker))
        {
            tiles.Add(target.GridPosition);
        }

        return tiles;
    }

    /// <summary>
    /// Executes a basic attack from attacker to target.
    /// </summary>
    /// <param name="attacker">The attacking unit.</param>
    /// <param name="target">The target unit.</param>
    /// <returns>The amount of damage dealt.</returns>
    public int ExecuteAttack(Unit attacker, Unit target)
    {
        if (!CanAttack(attacker, target)) return 0;

        // Calculate damage: AttackPower - Defense (minimum 1)
        int damage = CalculateDamage(attacker, target);

        // Apply damage to target
        int actualDamage = target.TakeDamage(damage);

        // Fire events
        OnAttackExecuted?.Invoke(attacker, target, actualDamage);
        OnUnitDamaged?.Invoke(target, actualDamage);

        // Check if target was defeated
        if (!target.IsAlive)
        {
            OnUnitDefeated?.Invoke(target);
        }

        return actualDamage;
    }

    /// <summary>
    /// Calculates damage from an attack.
    /// </summary>
    /// <param name="attacker">The attacking unit.</param>
    /// <param name="target">The target unit.</param>
    /// <returns>Calculated damage amount.</returns>
    public int CalculateDamage(Unit attacker, Unit target)
    {
        int baseDamage = attacker.AttackPower;

        // Apply defense reduction (minimum 1 damage)
        int damage = Math.Max(1, baseDamage - target.Defense);

        // Future: Apply passive trait modifiers here
        // e.g., BerserkerRage bonus, Flanker bonus, etc.

        return damage;
    }

    /// <summary>
    /// Gets the unit at a specific grid position.
    /// </summary>
    /// <param name="position">The grid position to check.</param>
    /// <returns>The unit at that position, or null if empty.</returns>
    public Unit? GetUnitAt(Point position)
    {
        return _units.FirstOrDefault(u => u.GridPosition == position && u.IsAlive);
    }

    /// <summary>
    /// Removes dead units from the game.
    /// </summary>
    public void CleanupDeadUnits()
    {
        _units.RemoveAll(u => !u.IsAlive);
    }
}
