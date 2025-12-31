using Microsoft.Xna.Framework;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Manages unit selection, movement, and attack targeting on the tactical grid.
/// Handles the select → highlight → move/attack flow.
/// </summary>
public class SelectionManager
{
    private readonly List<Unit> _units;
    private readonly HashSet<Point> _occupiedTiles;
    private CombatSystem? _combatSystem;

    /// <summary>
    /// Currently selected unit (null if nothing selected).
    /// </summary>
    public Unit? SelectedUnit { get; private set; }

    /// <summary>
    /// Tiles the selected unit can move to.
    /// </summary>
    public List<Point> ReachableTiles { get; private set; } = new();

    /// <summary>
    /// Tiles with enemies the selected unit can attack.
    /// </summary>
    public List<Point> AttackableTiles { get; private set; } = new();

    /// <summary>
    /// Event fired when a unit attacks another unit.
    /// </summary>
    public event Action<Unit, Unit, int>? OnAttackPerformed;

    /// <summary>
    /// Event fired when a unit is defeated.
    /// </summary>
    public event Action<Unit>? OnUnitDefeated;

    public SelectionManager(List<Unit> units)
    {
        _units = units;
        _occupiedTiles = new HashSet<Point>();
        RefreshOccupiedTiles();
    }

    /// <summary>
    /// Sets the combat system reference for handling attacks.
    /// </summary>
    public void SetCombatSystem(CombatSystem combatSystem)
    {
        _combatSystem = combatSystem;
    }

    /// <summary>
    /// Rebuilds the set of occupied tile positions.
    /// </summary>
    private void RefreshOccupiedTiles()
    {
        _occupiedTiles.Clear();
        foreach (var unit in _units)
        {
            _occupiedTiles.Add(unit.GridPosition);
        }
    }

    /// <summary>
    /// Gets the unit at a specific grid position (null if empty).
    /// </summary>
    public Unit? GetUnitAt(Point gridPos)
    {
        return _units.FirstOrDefault(u => u.GridPosition == gridPos);
    }

    /// <summary>
    /// Checks if a tile is occupied by any unit.
    /// </summary>
    public bool IsTileOccupied(Point gridPos)
    {
        return _occupiedTiles.Contains(gridPos);
    }

    /// <summary>
    /// Handles a click on a grid tile.
    /// - If clicking a player unit: select it
    /// - If clicking an attackable enemy: attack it
    /// - If clicking a reachable tile with unit selected: move unit
    /// - Otherwise: deselect
    /// </summary>
    public void HandleTileClick(Point clickedTile)
    {
        // Check if we clicked on a unit
        var clickedUnit = GetUnitAt(clickedTile);

        // If clicking a player unit, select it
        if (clickedUnit != null && clickedUnit.IsPlayerUnit)
        {
            SelectUnit(clickedUnit);
            return;
        }

        // If we have a selected unit...
        if (SelectedUnit != null)
        {
            // Check if we clicked on an attackable enemy
            if (AttackableTiles.Contains(clickedTile) && clickedUnit != null && !clickedUnit.IsPlayerUnit)
            {
                AttackTarget(clickedUnit);
                return;
            }

            // Check if we clicked a reachable tile to move there
            if (ReachableTiles.Contains(clickedTile))
            {
                MoveSelectedUnit(clickedTile);
                return;
            }
        }

        // Otherwise, deselect
        Deselect();
    }

    /// <summary>
    /// Executes an attack from the selected unit to the target.
    /// </summary>
    private void AttackTarget(Unit target)
    {
        if (SelectedUnit == null || _combatSystem == null) return;

        var attacker = SelectedUnit;
        int damage = _combatSystem.ExecuteAttack(attacker, target);

        // Fire attack event
        OnAttackPerformed?.Invoke(attacker, target, damage);

        // Check if target was defeated
        if (!target.IsAlive)
        {
            OnUnitDefeated?.Invoke(target);
            RefreshOccupiedTiles();
        }

        // Deselect after attacking (unit has used its action)
        Deselect();
    }

    /// <summary>
    /// Selects a unit and calculates its reachable and attackable tiles.
    /// </summary>
    private void SelectUnit(Unit unit)
    {
        SelectedUnit = unit;
        CalculateReachableTiles();
        CalculateAttackableTiles();
    }

    /// <summary>
    /// Deselects the current unit and clears all highlights.
    /// </summary>
    public void Deselect()
    {
        SelectedUnit = null;
        ReachableTiles.Clear();
        AttackableTiles.Clear();
    }

    /// <summary>
    /// Moves the selected unit to a target tile.
    /// </summary>
    private void MoveSelectedUnit(Point targetTile)
    {
        if (SelectedUnit == null) return;
        if (!Grid.IsValidPosition(targetTile)) return;
        if (IsTileOccupied(targetTile)) return;

        // Update unit position
        SelectedUnit.GridPosition = targetTile;

        // Refresh occupancy
        RefreshOccupiedTiles();

        // Deselect after moving
        Deselect();
    }

    /// <summary>
    /// Calculates all tiles the selected unit can reach.
    /// Excludes occupied tiles and the unit's current position.
    /// </summary>
    private void CalculateReachableTiles()
    {
        ReachableTiles.Clear();

        if (SelectedUnit == null) return;

        var tilesInRange = Grid.GetTilesInRange(SelectedUnit.GridPosition, SelectedUnit.MoveRange);

        foreach (var tile in tilesInRange)
        {
            // Only add unoccupied tiles
            if (!IsTileOccupied(tile))
            {
                ReachableTiles.Add(tile);
            }
        }
    }

    /// <summary>
    /// Calculates all tiles with enemies that the selected unit can attack.
    /// </summary>
    private void CalculateAttackableTiles()
    {
        AttackableTiles.Clear();

        if (SelectedUnit == null) return;

        // Get tiles within attack range
        var tilesInRange = Grid.GetTilesInRange(SelectedUnit.GridPosition, SelectedUnit.AttackRange);

        foreach (var tile in tilesInRange)
        {
            // Check if there's an enemy on this tile
            var unitOnTile = GetUnitAt(tile);
            if (unitOnTile != null && unitOnTile.IsAlive && unitOnTile.IsPlayerUnit != SelectedUnit.IsPlayerUnit)
            {
                AttackableTiles.Add(tile);
            }
        }
    }
}

