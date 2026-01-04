using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Represents a tactical battlefield with terrain data and shape.
/// Supports non-rectangular battlefield shapes and varied terrain.
/// </summary>
public class Battlefield
{
    /// <summary>
    /// Width of the battlefield in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Height of the battlefield in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// The terrain data for each tile. Void tiles are considered off-map.
    /// </summary>
    private readonly TerrainType[,] _terrain;

    /// <summary>
    /// Display name for this battlefield.
    /// </summary>
    public string Name { get; }

    public Battlefield(int width, int height, string name = "Battlefield")
    {
        Width = width;
        Height = height;
        Name = name;
        _terrain = new TerrainType[width, height];

        // Default to all Plains
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _terrain[x, y] = TerrainType.Plains;
            }
        }
    }

    /// <summary>
    /// Gets the terrain at a specific position.
    /// Returns Void if position is out of bounds.
    /// </summary>
    public TerrainType GetTerrain(Point pos) => GetTerrain(pos.X, pos.Y);

    /// <summary>
    /// Gets the terrain at a specific position.
    /// Returns Void if position is out of bounds.
    /// </summary>
    public TerrainType GetTerrain(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return TerrainType.Void;
        return _terrain[x, y];
    }

    /// <summary>
    /// Sets the terrain at a specific position.
    /// </summary>
    public void SetTerrain(int x, int y, TerrainType terrain)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            _terrain[x, y] = terrain;
    }

    /// <summary>
    /// Sets the terrain at a specific position.
    /// </summary>
    public void SetTerrain(Point pos, TerrainType terrain) => SetTerrain(pos.X, pos.Y, terrain);

    /// <summary>
    /// Returns true if the position is within bounds and not a Void tile.
    /// </summary>
    public bool IsValidTile(Point pos) => IsValidTile(pos.X, pos.Y);

    /// <summary>
    /// Returns true if the position is within bounds and not a Void tile.
    /// </summary>
    public bool IsValidTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return _terrain[x, y] != TerrainType.Void;
    }

    /// <summary>
    /// Returns true if units can move onto this tile.
    /// </summary>
    public bool IsPassable(Point pos) => GetTerrain(pos).IsPassable();

    /// <summary>
    /// Gets all valid (non-Void) tile positions on the battlefield.
    /// </summary>
    public List<Point> GetAllValidTiles()
    {
        var tiles = new List<Point>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_terrain[x, y] != TerrainType.Void)
                    tiles.Add(new Point(x, y));
            }
        }
        return tiles;
    }

    /// <summary>
    /// Gets all passable tile positions (tiles units can stand on).
    /// </summary>
    public List<Point> GetAllPassableTiles()
    {
        var tiles = new List<Point>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_terrain[x, y].IsPassable())
                    tiles.Add(new Point(x, y));
            }
        }
        return tiles;
    }

    /// <summary>
    /// Gets valid spawn positions for player units (left side of map).
    /// </summary>
    public List<Point> GetPlayerSpawnPositions()
    {
        var spawns = new List<Point>();
        // Check left columns first
        for (int x = 0; x < Width / 3; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (IsPassable(new Point(x, y)))
                    spawns.Add(new Point(x, y));
            }
        }
        return spawns;
    }

    /// <summary>
    /// Gets valid spawn positions for enemy units (right side of map).
    /// </summary>
    public List<Point> GetEnemySpawnPositions()
    {
        var spawns = new List<Point>();
        // Check right columns
        for (int x = Width * 2 / 3; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (IsPassable(new Point(x, y)))
                    spawns.Add(new Point(x, y));
            }
        }
        return spawns;
    }

    // ========================================
    // BATTLEFIELD GENERATORS (Static Factory Methods)
    // ========================================

    /// <summary>
    /// Creates a standard rectangular battlefield with some terrain variety.
    /// </summary>
    public static Battlefield CreateStandardBattlefield()
    {
        var bf = new Battlefield(10, 8, "Open Field");

        // Add some forests
        bf.SetTerrain(3, 2, TerrainType.Forest);
        bf.SetTerrain(3, 3, TerrainType.Forest);
        bf.SetTerrain(6, 4, TerrainType.Forest);
        bf.SetTerrain(6, 5, TerrainType.Forest);

        // Add hills
        bf.SetTerrain(5, 1, TerrainType.Hill);
        bf.SetTerrain(4, 6, TerrainType.Hill);

        return bf;
    }

    /// <summary>
    /// Creates a diamond-shaped battlefield (rotated square).
    /// </summary>
    public static Battlefield CreateDiamondBattlefield()
    {
        var bf = new Battlefield(9, 9, "Diamond Arena");

        // Create diamond shape using Void for corners
        int center = 4;
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                int dist = Math.Abs(x - center) + Math.Abs(y - center);
                if (dist > 4)
                    bf.SetTerrain(x, y, TerrainType.Void);
            }
        }

        // Add terrain features
        bf.SetTerrain(4, 4, TerrainType.Hill); // Center hill
        bf.SetTerrain(2, 4, TerrainType.Forest);
        bf.SetTerrain(6, 4, TerrainType.Forest);
        bf.SetTerrain(4, 2, TerrainType.Hazard);
        bf.SetTerrain(4, 6, TerrainType.Hazard);

        return bf;
    }

    /// <summary>
    /// Creates a battlefield with a river running through the middle.
    /// </summary>
    public static Battlefield CreateRiverCrossingBattlefield()
    {
        var bf = new Battlefield(10, 8, "River Crossing");

        // Create river down the middle (columns 4-5)
        for (int y = 0; y < 8; y++)
        {
            bf.SetTerrain(4, y, TerrainType.Water);
            bf.SetTerrain(5, y, TerrainType.Water);
        }

        // Create bridge/crossing points
        bf.SetTerrain(4, 3, TerrainType.Plains);
        bf.SetTerrain(5, 3, TerrainType.Plains);
        bf.SetTerrain(4, 6, TerrainType.Plains);
        bf.SetTerrain(5, 6, TerrainType.Plains);

        // Add mud near water
        bf.SetTerrain(3, 2, TerrainType.Mud);
        bf.SetTerrain(3, 4, TerrainType.Mud);
        bf.SetTerrain(6, 3, TerrainType.Mud);
        bf.SetTerrain(6, 5, TerrainType.Mud);

        // Add forests for cover
        bf.SetTerrain(1, 1, TerrainType.Forest);
        bf.SetTerrain(1, 6, TerrainType.Forest);
        bf.SetTerrain(8, 1, TerrainType.Forest);
        bf.SetTerrain(8, 6, TerrainType.Forest);

        return bf;
    }

    /// <summary>
    /// Creates an L-shaped battlefield.
    /// </summary>
    public static Battlefield CreateLShapedBattlefield()
    {
        var bf = new Battlefield(10, 8, "Corner Ruins");

        // Void out the top-right corner to create L shape
        for (int x = 6; x < 10; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                bf.SetTerrain(x, y, TerrainType.Void);
            }
        }

        // Add hazards near the corner
        bf.SetTerrain(5, 3, TerrainType.Hazard);
        bf.SetTerrain(6, 4, TerrainType.Hazard);

        // Add forests
        bf.SetTerrain(2, 2, TerrainType.Forest);
        bf.SetTerrain(2, 3, TerrainType.Forest);
        bf.SetTerrain(3, 6, TerrainType.Forest);

        // Add hills
        bf.SetTerrain(8, 6, TerrainType.Hill);
        bf.SetTerrain(7, 7, TerrainType.Hill);

        return bf;
    }

    /// <summary>
    /// Creates a battlefield with a central island and water moat.
    /// </summary>
    public static Battlefield CreateIslandBattlefield()
    {
        var bf = new Battlefield(11, 9, "Island Fortress");

        // Create water moat around center
        for (int x = 0; x < 11; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                int dx = Math.Abs(x - 5);
                int dy = Math.Abs(y - 4);

                // Inner ring of water
                if (dx + dy == 3)
                    bf.SetTerrain(x, y, TerrainType.Water);
            }
        }

        // Bridges to the center
        bf.SetTerrain(2, 4, TerrainType.Plains);
        bf.SetTerrain(8, 4, TerrainType.Plains);
        bf.SetTerrain(5, 1, TerrainType.Plains);
        bf.SetTerrain(5, 7, TerrainType.Plains);

        // Central hill
        bf.SetTerrain(5, 4, TerrainType.Hill);

        // Void corners to make it more interesting
        bf.SetTerrain(0, 0, TerrainType.Void);
        bf.SetTerrain(10, 0, TerrainType.Void);
        bf.SetTerrain(0, 8, TerrainType.Void);
        bf.SetTerrain(10, 8, TerrainType.Void);

        return bf;
    }

    /// <summary>
    /// Creates a narrow canyon/corridor battlefield.
    /// </summary>
    public static Battlefield CreateCanyonBattlefield()
    {
        var bf = new Battlefield(12, 7, "Mountain Pass");

        // Create canyon walls (void on top and bottom)
        for (int x = 0; x < 12; x++)
        {
            // Top wall with some variation
            if (x < 4 || x > 7)
            {
                bf.SetTerrain(x, 0, TerrainType.Void);
                if (x < 3 || x > 8)
                    bf.SetTerrain(x, 1, TerrainType.Void);
            }

            // Bottom wall with some variation
            if (x < 3 || x > 8)
            {
                bf.SetTerrain(x, 6, TerrainType.Void);
                if (x < 2 || x > 9)
                    bf.SetTerrain(x, 5, TerrainType.Void);
            }
        }

        // Add rocky hazards in the canyon
        bf.SetTerrain(5, 3, TerrainType.Hazard);
        bf.SetTerrain(7, 3, TerrainType.Hazard);

        // Add hills at the ends
        bf.SetTerrain(1, 3, TerrainType.Hill);
        bf.SetTerrain(10, 3, TerrainType.Hill);

        // Add ice patches
        bf.SetTerrain(4, 2, TerrainType.Ice);
        bf.SetTerrain(4, 4, TerrainType.Ice);
        bf.SetTerrain(8, 2, TerrainType.Ice);
        bf.SetTerrain(8, 4, TerrainType.Ice);

        return bf;
    }

    /// <summary>
    /// Creates a hexagon-approximated battlefield.
    /// </summary>
    public static Battlefield CreateHexagonBattlefield()
    {
        var bf = new Battlefield(10, 8, "Hexagon Arena");

        // Void out corners to approximate hexagon
        // Top-left
        bf.SetTerrain(0, 0, TerrainType.Void);
        bf.SetTerrain(1, 0, TerrainType.Void);
        bf.SetTerrain(0, 1, TerrainType.Void);

        // Top-right
        bf.SetTerrain(9, 0, TerrainType.Void);
        bf.SetTerrain(8, 0, TerrainType.Void);
        bf.SetTerrain(9, 1, TerrainType.Void);

        // Bottom-left
        bf.SetTerrain(0, 7, TerrainType.Void);
        bf.SetTerrain(1, 7, TerrainType.Void);
        bf.SetTerrain(0, 6, TerrainType.Void);

        // Bottom-right
        bf.SetTerrain(9, 7, TerrainType.Void);
        bf.SetTerrain(8, 7, TerrainType.Void);
        bf.SetTerrain(9, 6, TerrainType.Void);

        // Add central features
        bf.SetTerrain(4, 3, TerrainType.Forest);
        bf.SetTerrain(5, 3, TerrainType.Forest);
        bf.SetTerrain(4, 4, TerrainType.Forest);
        bf.SetTerrain(5, 4, TerrainType.Forest);

        // Add flanking hills
        bf.SetTerrain(2, 2, TerrainType.Hill);
        bf.SetTerrain(7, 5, TerrainType.Hill);

        // Add hazards
        bf.SetTerrain(3, 6, TerrainType.Hazard);
        bf.SetTerrain(6, 1, TerrainType.Hazard);

        return bf;
    }

    /// <summary>
    /// Gets a random battlefield from the available types.
    /// </summary>
    public static Battlefield CreateRandomBattlefield(Random random)
    {
        int type = random.Next(7);
        return type switch
        {
            0 => CreateStandardBattlefield(),
            1 => CreateDiamondBattlefield(),
            2 => CreateRiverCrossingBattlefield(),
            3 => CreateLShapedBattlefield(),
            4 => CreateIslandBattlefield(),
            5 => CreateCanyonBattlefield(),
            6 => CreateHexagonBattlefield(),
            _ => CreateStandardBattlefield()
        };
    }

    /// <summary>
    /// Gets a battlefield appropriate for the given difficulty.
    /// </summary>
    public static Battlefield CreateBattlefieldForDifficulty(int difficulty, Random random)
    {
        // Easier battles get simpler maps
        if (difficulty <= 1)
        {
            return random.Next(2) == 0 ? CreateStandardBattlefield() : CreateHexagonBattlefield();
        }
        else if (difficulty == 2)
        {
            int type = random.Next(3);
            return type switch
            {
                0 => CreateRiverCrossingBattlefield(),
                1 => CreateLShapedBattlefield(),
                _ => CreateHexagonBattlefield()
            };
        }
        else
        {
            // Hard battles get the most challenging maps
            int type = random.Next(4);
            return type switch
            {
                0 => CreateDiamondBattlefield(),
                1 => CreateIslandBattlefield(),
                2 => CreateCanyonBattlefield(),
                _ => CreateRiverCrossingBattlefield()
            };
        }
    }
}
