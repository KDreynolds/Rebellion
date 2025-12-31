using Microsoft.Xna.Framework;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Simple enemy AI that handles enemy unit actions during their turn.
/// Enemies will try to attack nearby player units, or move toward the closest one.
/// </summary>
public class EnemyAI
{
    private readonly List<Unit> _units;
    private readonly CombatSystem _combatSystem;

    /// <summary>
    /// Event fired when an enemy attacks a player unit.
    /// </summary>
    public event Action<Unit, Unit, int>? OnEnemyAttack;

    /// <summary>
    /// Event fired when an enemy moves.
    /// </summary>
    public event Action<Unit, Point, Point>? OnEnemyMove;

    public EnemyAI(List<Unit> units, CombatSystem combatSystem)
    {
        _units = units;
        _combatSystem = combatSystem;
    }

    /// <summary>
    /// Executes a turn for all enemy units.
    /// Each enemy will try to attack if possible, otherwise move toward the closest player.
    /// </summary>
    public void ExecuteEnemyTurn()
    {
        var enemies = _units.Where(u => !u.IsPlayerUnit && u.IsAlive).ToList();

        foreach (var enemy in enemies)
        {
            ExecuteUnitAction(enemy);
        }
    }

    /// <summary>
    /// Executes action for a single enemy unit.
    /// </summary>
    private void ExecuteUnitAction(Unit enemy)
    {
        // First, try to attack a player unit in range
        var target = FindAttackTarget(enemy);
        if (target != null)
        {
            int damage = _combatSystem.ExecuteAttack(enemy, target);
            OnEnemyAttack?.Invoke(enemy, target, damage);
            return;
        }

        // If no target in range, move toward closest player
        var closestPlayer = FindClosestPlayerUnit(enemy);
        if (closestPlayer != null)
        {
            MoveTowardTarget(enemy, closestPlayer.GridPosition);
        }
    }

    /// <summary>
    /// Finds a player unit within attack range of the enemy.
    /// </summary>
    private Unit? FindAttackTarget(Unit enemy)
    {
        var targets = _combatSystem.GetValidTargets(enemy);
        
        if (targets.Count == 0) return null;

        // Prefer targets with lower HP (finish them off)
        return targets.OrderBy(t => t.CurrentHP).First();
    }

    /// <summary>
    /// Finds the closest living player unit to the enemy.
    /// </summary>
    private Unit? FindClosestPlayerUnit(Unit enemy)
    {
        return _units
            .Where(u => u.IsPlayerUnit && u.IsAlive)
            .OrderBy(u => Grid.ManhattanDistance(enemy.GridPosition, u.GridPosition))
            .FirstOrDefault();
    }

    /// <summary>
    /// Moves the enemy one step toward the target position.
    /// Uses simple pathfinding (move in the direction with the largest gap).
    /// </summary>
    private void MoveTowardTarget(Unit enemy, Point targetPos)
    {
        var oldPos = enemy.GridPosition;
        var possibleMoves = GetValidMoves(enemy);

        if (possibleMoves.Count == 0) return;

        // Find the move that gets us closest to the target
        Point bestMove = possibleMoves
            .OrderBy(p => Grid.ManhattanDistance(p, targetPos))
            .First();

        // Only move if it actually gets us closer
        if (Grid.ManhattanDistance(bestMove, targetPos) < Grid.ManhattanDistance(oldPos, targetPos))
        {
            enemy.GridPosition = bestMove;
            OnEnemyMove?.Invoke(enemy, oldPos, bestMove);
        }
    }

    /// <summary>
    /// Gets all valid movement positions for a unit.
    /// </summary>
    private List<Point> GetValidMoves(Unit unit)
    {
        var moves = new List<Point>();
        var tilesInRange = Grid.GetTilesInRange(unit.GridPosition, unit.MoveRange);
        var occupiedTiles = _units.Where(u => u.IsAlive).Select(u => u.GridPosition).ToHashSet();

        foreach (var tile in tilesInRange)
        {
            if (!occupiedTiles.Contains(tile))
            {
                moves.Add(tile);
            }
        }

        return moves;
    }
}
