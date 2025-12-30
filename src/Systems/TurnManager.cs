using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Manages turn-based combat flow.
/// Handles player turn, enemy turn, and unit action tracking.
/// </summary>
public class TurnManager
{
    private readonly List<Unit> _allUnits;
    private readonly List<Unit> _playerUnits;
    private readonly List<Unit> _enemyUnits;

    /// <summary>
    /// Current turn phase.
    /// </summary>
    public TurnPhase CurrentPhase { get; private set; }

    /// <summary>
    /// Units that have acted this turn.
    /// </summary>
    private readonly HashSet<Unit> _actedThisTurn = new();

    /// <summary>
    /// Whether it's currently the player's turn.
    /// </summary>
    public bool IsPlayerTurn => CurrentPhase == TurnPhase.PlayerTurn;

    /// <summary>
    /// Whether it's currently the enemy turn.
    /// </summary>
    public bool IsEnemyTurn => CurrentPhase == TurnPhase.EnemyTurn;

    /// <summary>
    /// Whether the current phase is processing (e.g., enemy AI is acting).
    /// </summary>
    public bool IsProcessing { get; private set; }

    public TurnManager(List<Unit> allUnits)
    {
        _allUnits = allUnits;
        _playerUnits = allUnits.Where(u => u.IsPlayerUnit && u.IsAlive).ToList();
        _enemyUnits = allUnits.Where(u => !u.IsPlayerUnit && u.IsAlive).ToList();
        CurrentPhase = TurnPhase.PlayerTurn;
    }

    /// <summary>
    /// Checks if a unit can act this turn.
    /// </summary>
    public bool CanUnitAct(Unit unit)
    {
        if (unit == null || !unit.IsAlive) return false;
        if (_actedThisTurn.Contains(unit)) return false;

        if (unit.IsPlayerUnit)
        {
            // Player units can act during player turn when not processing
            return IsPlayerTurn && !IsProcessing;
        }
        else
        {
            // Enemy units can act during enemy turn when processing (AI is running)
            return IsEnemyTurn && IsProcessing;
        }
    }

    /// <summary>
    /// Marks a unit as having acted this turn.
    /// </summary>
    public void MarkUnitActed(Unit unit)
    {
        if (unit != null)
        {
            _actedThisTurn.Add(unit);
        }
    }

    /// <summary>
    /// Checks if all player units have acted this turn.
    /// </summary>
    public bool AllPlayerUnitsActed()
    {
        return _playerUnits.All(u => !u.IsAlive || _actedThisTurn.Contains(u));
    }

    /// <summary>
    /// Checks if all enemy units have acted this turn.
    /// </summary>
    public bool AllEnemyUnitsActed()
    {
        return _enemyUnits.All(u => !u.IsAlive || _actedThisTurn.Contains(u));
    }

    /// <summary>
    /// Ends the current turn and advances to the next phase.
    /// </summary>
    public void EndTurn()
    {
        if (IsProcessing) return;

        _actedThisTurn.Clear();

        switch (CurrentPhase)
        {
            case TurnPhase.PlayerTurn:
                CurrentPhase = TurnPhase.EnemyTurn;
                IsProcessing = true;
                break;

            case TurnPhase.EnemyTurn:
                CurrentPhase = TurnPhase.PlayerTurn;
                RefreshUnitLists(); // Update alive units
                break;
        }
    }

    /// <summary>
    /// Marks the enemy turn as complete (called after all enemies have acted).
    /// </summary>
    public void CompleteEnemyTurn()
    {
        if (CurrentPhase == TurnPhase.EnemyTurn)
        {
            IsProcessing = false;
            EndTurn(); // This will switch to player turn
        }
    }

    /// <summary>
    /// Refreshes the player and enemy unit lists (removes dead units).
    /// </summary>
    public void RefreshUnitLists()
    {
        _playerUnits.Clear();
        _playerUnits.AddRange(_allUnits.Where(u => u.IsPlayerUnit && u.IsAlive));

        _enemyUnits.Clear();
        _enemyUnits.AddRange(_allUnits.Where(u => !u.IsPlayerUnit && u.IsAlive));
    }

    /// <summary>
    /// Gets all alive player units.
    /// </summary>
    public List<Unit> GetPlayerUnits()
    {
        RefreshUnitLists(); // Ensure lists are up to date
        return _playerUnits.ToList();
    }

    /// <summary>
    /// Gets all alive enemy units.
    /// </summary>
    public List<Unit> GetEnemyUnits()
    {
        RefreshUnitLists(); // Ensure lists are up to date
        return _enemyUnits.ToList();
    }

    /// <summary>
    /// Checks if the battle is over (all player units or all enemy units are dead).
    /// </summary>
    public bool IsBattleOver()
    {
        var playerAlive = _playerUnits.Any(u => u.IsAlive);
        var enemyAlive = _enemyUnits.Any(u => u.IsAlive);
        return !playerAlive || !enemyAlive;
    }

    /// <summary>
    /// Gets the battle result if the battle is over.
    /// </summary>
    public BattleResult? GetBattleResult()
    {
        if (!IsBattleOver()) return null;

        var playerAlive = _playerUnits.Any(u => u.IsAlive);
        return playerAlive ? BattleResult.PlayerVictory : BattleResult.EnemyVictory;
    }
}

/// <summary>
/// Represents the current phase of combat.
/// </summary>
public enum TurnPhase
{
    /// <summary>
    /// Player's turn - player can select and move/attack with their units.
    /// </summary>
    PlayerTurn,

    /// <summary>
    /// Enemy's turn - enemies act automatically.
    /// </summary>
    EnemyTurn
}

/// <summary>
/// Result of a completed battle.
/// </summary>
public enum BattleResult
{
    PlayerVictory,
    EnemyVictory
}

