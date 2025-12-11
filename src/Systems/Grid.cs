using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Core grid system providing constants and coordinate conversion utilities.
/// The tactical battlefield is an 8×8 grid of 64×64 pixel tiles.
/// </summary>
public static class Grid
{
    /// <summary>
    /// Number of tiles in each dimension (8×8 grid).
    /// </summary>
    public const int GridSize = 8;

    /// <summary>
    /// Size of each tile in pixels.
    /// </summary>
    public const int TileSize = 64;

    /// <summary>
    /// Offset from left edge of screen to start of grid (for centering).
    /// </summary>
    public const int OffsetX = 64;

    /// <summary>
    /// Offset from top edge of screen to start of grid.
    /// </summary>
    public const int OffsetY = 64;

    /// <summary>
    /// Total pixel width of the grid.
    /// </summary>
    public static int TotalWidth => GridSize * TileSize;

    /// <summary>
    /// Total pixel height of the grid.
    /// </summary>
    public static int TotalHeight => GridSize * TileSize;

    /// <summary>
    /// Converts a grid coordinate to the top-left pixel position of that tile.
    /// </summary>
    public static Vector2 GridToPixel(Point gridPos)
    {
        return new Vector2(
            OffsetX + gridPos.X * TileSize,
            OffsetY + gridPos.Y * TileSize
        );
    }

    /// <summary>
    /// Converts a grid coordinate to the center pixel position of that tile.
    /// </summary>
    public static Vector2 GridToPixelCenter(Point gridPos)
    {
        return new Vector2(
            OffsetX + gridPos.X * TileSize + TileSize / 2,
            OffsetY + gridPos.Y * TileSize + TileSize / 2
        );
    }

    /// <summary>
    /// Converts a pixel position to grid coordinates.
    /// Returns null if the position is outside the grid bounds.
    /// </summary>
    public static Point? PixelToGrid(Vector2 pixelPos)
    {
        int gridX = (int)(pixelPos.X - OffsetX) / TileSize;
        int gridY = (int)(pixelPos.Y - OffsetY) / TileSize;

        if (IsValidPosition(new Point(gridX, gridY)) &&
            pixelPos.X >= OffsetX && pixelPos.Y >= OffsetY)
        {
            return new Point(gridX, gridY);
        }

        return null;
    }

    /// <summary>
    /// Checks if a grid position is within the valid grid bounds.
    /// </summary>
    public static bool IsValidPosition(Point gridPos)
    {
        return gridPos.X >= 0 && gridPos.X < GridSize &&
               gridPos.Y >= 0 && gridPos.Y < GridSize;
    }

    /// <summary>
    /// Calculates Manhattan distance between two grid positions.
    /// </summary>
    public static int ManhattanDistance(Point a, Point b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Returns all valid grid positions within Manhattan distance of a center point.
    /// </summary>
    public static List<Point> GetTilesInRange(Point center, int range)
    {
        var tiles = new List<Point>();

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                var pos = new Point(x, y);
                if (ManhattanDistance(center, pos) <= range && pos != center)
                {
                    tiles.Add(pos);
                }
            }
        }

        return tiles;
    }
}

