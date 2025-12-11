using Microsoft.Xna.Framework;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Manages unit selection and movement on the tactical grid.
/// Handles the select → highlight → move flow.
/// </summary>
public class SelectionManager
{
    private readonly List<Unit> _units;
    private readonly HashSet<Point> _occupiedTiles;

    /// <summary>
    /// Currently selected unit (null if nothing selected).
    /// </summary>
    public Unit? SelectedUnit { get; private set; }

    /// <summary>
    /// Tiles the selected unit can move to.
    /// </summary>
    public List<Point> ReachableTiles { get; private set; } = new();

    public SelectionManager(List<Unit> units)
    {
        _units = units;
        _occupiedTiles = new HashSet<Point>();
        RefreshOccupiedTiles();
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
    /// - If clicking a reachable tile with unit selected: move unit
    /// - Otherwise: deselect
    /// </summary>
    public void HandleTileClick(Point clickedTile)
    {
        // Check if we clicked on a player unit
        var clickedUnit = GetUnitAt(clickedTile);

        if (clickedUnit != null && clickedUnit.IsPlayerUnit)
        {
            // Select this unit
            SelectUnit(clickedUnit);
            return;
        }

        // If we have a selected unit and clicked a reachable tile, move there
        if (SelectedUnit != null && ReachableTiles.Contains(clickedTile))
        {
            MoveSelectedUnit(clickedTile);
            return;
        }

        // Otherwise, deselect
        Deselect();
    }

    /// <summary>
    /// Selects a unit and calculates its reachable tiles.
    /// </summary>
    private void SelectUnit(Unit unit)
    {
        SelectedUnit = unit;
        CalculateReachableTiles();
    }

    /// <summary>
    /// Deselects the current unit and clears reachable tiles.
    /// </summary>
    public void Deselect()
    {
        SelectedUnit = null;
        ReachableTiles.Clear();
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
}

