using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Represents a tactical unit on the battlefield.
/// Units have a grid position, movement range, and visual properties.
/// </summary>
public class Unit
{
    /// <summary>
    /// Current position on the grid (column, row).
    /// </summary>
    public Point GridPosition { get; set; }

    /// <summary>
    /// Maximum tiles this unit can move in a single turn (Manhattan distance).
    /// </summary>
    public int MoveRange { get; set; }

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

    public Unit(string name, Point gridPosition, int moveRange, Color color, bool isPlayerUnit = true)
    {
        Name = name;
        GridPosition = gridPosition;
        MoveRange = moveRange;
        Color = color;
        IsPlayerUnit = isPlayerUnit;
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
}

