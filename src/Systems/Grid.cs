using Microsoft.Xna.Framework;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Core grid system providing coordinate conversion utilities and battlefield access.
/// Supports dynamic battlefield sizes and terrain-aware movement calculations.
/// </summary>
public static class Grid
{
    /// <summary>
    /// Size of each tile in pixels.
    /// </summary>
    public const int TileSize = 64;

    /// <summary>
    /// Offset from left edge of screen to start of grid (for centering).
    /// </summary>
    public const int OffsetX = 48;

    /// <summary>
    /// Offset from top edge of screen to start of grid.
    /// </summary>
    public const int OffsetY = 64;

    /// <summary>
    /// The current battlefield being used (null for legacy 8x8 mode).
    /// </summary>
    public static Battlefield? CurrentBattlefield { get; set; }

    /// <summary>
    /// Width of the current battlefield in tiles.
    /// </summary>
    public static int Width => CurrentBattlefield?.Width ?? 8;

    /// <summary>
    /// Height of the current battlefield in tiles.
    /// </summary>
    public static int Height => CurrentBattlefield?.Height ?? 8;

    /// <summary>
    /// Total pixel width of the grid.
    /// </summary>
    public static int TotalWidth => Width * TileSize;

    /// <summary>
    /// Total pixel height of the grid.
    /// </summary>
    public static int TotalHeight => Height * TileSize;

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
    /// Returns null if the position is outside the grid bounds or on a Void tile.
    /// </summary>
    public static Point? PixelToGrid(Vector2 pixelPos)
    {
        if (pixelPos.X < OffsetX || pixelPos.Y < OffsetY)
            return null;

        int gridX = (int)(pixelPos.X - OffsetX) / TileSize;
        int gridY = (int)(pixelPos.Y - OffsetY) / TileSize;

        var pos = new Point(gridX, gridY);
        if (IsValidPosition(pos))
        {
            return pos;
        }

        return null;
    }

    /// <summary>
    /// Checks if a grid position is within the valid grid bounds and not a Void tile.
    /// </summary>
    public static bool IsValidPosition(Point gridPos)
    {
        if (CurrentBattlefield != null)
        {
            return CurrentBattlefield.IsValidTile(gridPos);
        }

        // Legacy fallback for 8x8 grid
        return gridPos.X >= 0 && gridPos.X < 8 &&
               gridPos.Y >= 0 && gridPos.Y < 8;
    }

    /// <summary>
    /// Checks if a grid position is passable (units can move there).
    /// </summary>
    public static bool IsPassable(Point gridPos)
    {
        if (CurrentBattlefield != null)
        {
            return CurrentBattlefield.IsPassable(gridPos);
        }
        return IsValidPosition(gridPos);
    }

    /// <summary>
    /// Gets the terrain at a specific position.
    /// </summary>
    public static TerrainType GetTerrain(Point gridPos)
    {
        return CurrentBattlefield?.GetTerrain(gridPos) ?? TerrainType.Plains;
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
    /// Only includes tiles that exist on the battlefield (not Void).
    /// </summary>
    public static List<Point> GetTilesInRange(Point center, int range)
    {
        var tiles = new List<Point>();

        // Use larger search area based on battlefield size
        int minX = Math.Max(0, center.X - range);
        int maxX = Math.Min(Width - 1, center.X + range);
        int minY = Math.Max(0, center.Y - range);
        int maxY = Math.Min(Height - 1, center.Y + range);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var pos = new Point(x, y);
                if (ManhattanDistance(center, pos) <= range && pos != center && IsValidPosition(pos))
                {
                    tiles.Add(pos);
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns all tiles reachable with a given movement budget, accounting for terrain movement costs.
    /// Uses flood-fill pathfinding to accurately calculate reachable tiles.
    /// </summary>
    public static List<Point> GetReachableTiles(Point start, int movementBudget, HashSet<Point> occupiedTiles)
    {
        var reachable = new List<Point>();
        var visited = new Dictionary<Point, int>(); // Position -> remaining movement
        var queue = new Queue<(Point pos, int remaining)>();

        visited[start] = movementBudget;
        queue.Enqueue((start, movementBudget));

        while (queue.Count > 0)
        {
            var (current, remaining) = queue.Dequeue();

            // Check all 4 adjacent tiles
            Point[] neighbors = {
                new Point(current.X - 1, current.Y),
                new Point(current.X + 1, current.Y),
                new Point(current.X, current.Y - 1),
                new Point(current.X, current.Y + 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (!IsPassable(neighbor)) continue;

                int moveCost = GetTerrain(neighbor).GetMovementCost();
                int newRemaining = remaining - moveCost;

                if (newRemaining < 0) continue;

                // Only visit if we haven't been here with more movement remaining
                if (visited.TryGetValue(neighbor, out int prevRemaining) && prevRemaining >= newRemaining)
                    continue;

                visited[neighbor] = newRemaining;
                queue.Enqueue((neighbor, newRemaining));

                // Add to reachable if not occupied and not the start
                if (!occupiedTiles.Contains(neighbor) && neighbor != start)
                {
                    if (!reachable.Contains(neighbor))
                        reachable.Add(neighbor);
                }
            }
        }

        return reachable;
    }

    /// <summary>
    /// Gets the 4-directional neighbors of a tile that are valid positions.
    /// </summary>
    public static List<Point> GetNeighbors(Point pos)
    {
        var neighbors = new List<Point>();
        Point[] candidates = {
            new Point(pos.X - 1, pos.Y),
            new Point(pos.X + 1, pos.Y),
            new Point(pos.X, pos.Y - 1),
            new Point(pos.X, pos.Y + 1)
        };

        foreach (var candidate in candidates)
        {
            if (IsValidPosition(candidate))
                neighbors.Add(candidate);
        }

        return neighbors;
    }
}

