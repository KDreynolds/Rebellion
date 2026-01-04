using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegacyOfTheShatteredCrown.Systems;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Rendering;

/// <summary>
/// Renders the tactical grid with terrain types, tile highlights for hover, selection, and movement range.
/// Uses a single-pixel texture for all shape drawing.
/// </summary>
public class GridRenderer
{
    private readonly Texture2D _pixelTexture;

    // Highlight colors
    private readonly Color _gridLineColor = new(60, 60, 70);
    private readonly Color _hoverColor = new(100, 100, 120, 180);
    private readonly Color _selectedColor = new(255, 200, 80, 200);
    private readonly Color _reachableColor = new(80, 180, 120, 150);
    private readonly Color _attackableColor = new(200, 80, 80, 180);

    // Terrain base colors (light variant for checkerboard)
    private readonly Dictionary<TerrainType, (Color light, Color dark)> _terrainColors = new()
    {
        { TerrainType.Plains, (new Color(55, 65, 45), new Color(45, 55, 35)) },
        { TerrainType.Forest, (new Color(35, 75, 35), new Color(25, 60, 25)) },
        { TerrainType.Hill, (new Color(85, 75, 55), new Color(70, 60, 45)) },
        { TerrainType.Hazard, (new Color(90, 45, 30), new Color(75, 35, 25)) },
        { TerrainType.Water, (new Color(35, 55, 95), new Color(25, 45, 80)) },
        { TerrainType.Mud, (new Color(65, 55, 40), new Color(55, 45, 30)) },
        { TerrainType.Ice, (new Color(140, 160, 180), new Color(120, 140, 160)) },
        { TerrainType.Void, (Color.Transparent, Color.Transparent) }
    };

    public GridRenderer(GraphicsDevice graphicsDevice)
    {
        // Create a 1x1 white pixel texture for drawing shapes
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    /// <summary>
    /// Draws the complete tactical grid with all highlights.
    /// </summary>
    public void Draw(
        SpriteBatch spriteBatch,
        Point? hoveredTile,
        Point? selectedUnitPosition,
        List<Point> reachableTiles,
        List<Point>? attackableTiles = null)
    {
        // Draw base grid tiles with terrain
        DrawGridTiles(spriteBatch);

        // Draw reachable tile highlights (movement)
        foreach (var tile in reachableTiles)
        {
            DrawTileHighlight(spriteBatch, tile, _reachableColor);
        }

        // Draw attackable tile highlights (enemies in range)
        if (attackableTiles != null)
        {
            foreach (var tile in attackableTiles)
            {
                DrawTileHighlight(spriteBatch, tile, _attackableColor);
            }
        }

        // Draw selected unit highlight
        if (selectedUnitPosition.HasValue)
        {
            DrawTileHighlight(spriteBatch, selectedUnitPosition.Value, _selectedColor);
        }

        // Draw hover highlight (on top)
        if (hoveredTile.HasValue && Grid.IsValidPosition(hoveredTile.Value))
        {
            DrawTileHighlight(spriteBatch, hoveredTile.Value, _hoverColor);
        }

        // Draw grid lines (only on valid tiles)
        DrawGridLines(spriteBatch);

        // Draw terrain decorations
        DrawTerrainDecorations(spriteBatch);
    }

    /// <summary>
    /// Draws the grid tiles with terrain-appropriate colors.
    /// </summary>
    private void DrawGridTiles(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                var gridPos = new Point(x, y);
                var terrain = Grid.GetTerrain(gridPos);

                // Skip void tiles
                if (terrain == TerrainType.Void) continue;

                var pos = Grid.GridToPixel(gridPos);
                var (light, dark) = _terrainColors.GetValueOrDefault(terrain, (new Color(45, 45, 55), new Color(35, 35, 45)));
                var color = (x + y) % 2 == 0 ? light : dark;

                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle((int)pos.X, (int)pos.Y, Grid.TileSize, Grid.TileSize),
                    color
                );
            }
        }
    }

    /// <summary>
    /// Draws terrain decorations (icons/symbols for special terrain).
    /// </summary>
    private void DrawTerrainDecorations(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                var gridPos = new Point(x, y);
                var terrain = Grid.GetTerrain(gridPos);
                var pos = Grid.GridToPixel(gridPos);

                switch (terrain)
                {
                    case TerrainType.Forest:
                        DrawTreeSymbol(spriteBatch, pos);
                        break;
                    case TerrainType.Hill:
                        DrawHillSymbol(spriteBatch, pos);
                        break;
                    case TerrainType.Hazard:
                        DrawHazardSymbol(spriteBatch, pos);
                        break;
                    case TerrainType.Water:
                        DrawWaterSymbol(spriteBatch, pos);
                        break;
                    case TerrainType.Mud:
                        DrawMudSymbol(spriteBatch, pos);
                        break;
                    case TerrainType.Ice:
                        DrawIceSymbol(spriteBatch, pos);
                        break;
                }
            }
        }
    }

    private void DrawTreeSymbol(SpriteBatch spriteBatch, Vector2 pos)
    {
        // Simple tree: triangle on trunk
        var centerX = (int)pos.X + Grid.TileSize / 2;
        var baseY = (int)pos.Y + Grid.TileSize - 12;
        var trunkColor = new Color(80, 55, 30, 150);
        var leafColor = new Color(30, 90, 30, 180);

        // Trunk
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 3, baseY - 8, 6, 12), trunkColor);

        // Canopy (layered triangles approximation using rectangles)
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 12, baseY - 14, 24, 4), leafColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 10, baseY - 20, 20, 6), leafColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 6, baseY - 26, 12, 6), leafColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 2, baseY - 30, 4, 4), leafColor);
    }

    private void DrawHillSymbol(SpriteBatch spriteBatch, Vector2 pos)
    {
        // Simple hill: curved mound
        var centerX = (int)pos.X + Grid.TileSize / 2;
        var baseY = (int)pos.Y + Grid.TileSize - 14;
        var hillColor = new Color(110, 95, 70, 150);

        // Mound shape (stacked rectangles)
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 18, baseY, 36, 6), hillColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 14, baseY - 6, 28, 6), hillColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 8, baseY - 12, 16, 6), hillColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 3, baseY - 16, 6, 4), hillColor);
    }

    private void DrawHazardSymbol(SpriteBatch spriteBatch, Vector2 pos)
    {
        // Hazard: flame/spike symbols
        var centerX = (int)pos.X + Grid.TileSize / 2;
        var centerY = (int)pos.Y + Grid.TileSize / 2;
        var flameColor = new Color(220, 120, 40, 200);
        var coreColor = new Color(255, 200, 80, 180);

        // Flames (vertical bars with varying heights)
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 10, centerY - 6, 4, 16), flameColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 2, centerY - 12, 4, 22), flameColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX + 6, centerY - 4, 4, 14), flameColor);

        // Bright cores
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 1, centerY - 2, 2, 8), coreColor);
    }

    private void DrawWaterSymbol(SpriteBatch spriteBatch, Vector2 pos)
    {
        // Water: wave lines
        var baseX = (int)pos.X + 10;
        var centerY = (int)pos.Y + Grid.TileSize / 2;
        var waveColor = new Color(80, 120, 180, 150);

        // Horizontal wave lines
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX, centerY - 8, 44, 2), waveColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX + 4, centerY, 36, 2), waveColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX, centerY + 8, 44, 2), waveColor);
    }

    private void DrawMudSymbol(SpriteBatch spriteBatch, Vector2 pos)
    {
        // Mud: dots/splatter
        var baseX = (int)pos.X + 12;
        var baseY = (int)pos.Y + 12;
        var mudColor = new Color(90, 70, 50, 150);

        // Scattered dots
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX + 5, baseY + 5, 6, 6), mudColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX + 25, baseY + 8, 8, 5), mudColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX + 12, baseY + 22, 7, 7), mudColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(baseX + 32, baseY + 28, 5, 5), mudColor);
    }

    private void DrawIceSymbol(SpriteBatch spriteBatch, Vector2 pos)
    {
        // Ice: crystalline pattern
        var centerX = (int)pos.X + Grid.TileSize / 2;
        var centerY = (int)pos.Y + Grid.TileSize / 2;
        var iceColor = new Color(200, 220, 255, 180);

        // Cross pattern
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 1, centerY - 14, 2, 28), iceColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 14, centerY - 1, 28, 2), iceColor);

        // Diagonal hints
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 8, centerY - 8, 4, 4), iceColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX + 4, centerY - 8, 4, 4), iceColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX - 8, centerY + 4, 4, 4), iceColor);
        spriteBatch.Draw(_pixelTexture, new Rectangle(centerX + 4, centerY + 4, 4, 4), iceColor);
    }

    /// <summary>
    /// Draws grid lines, only around valid tiles.
    /// </summary>
    private void DrawGridLines(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                var gridPos = new Point(x, y);
                if (!Grid.IsValidPosition(gridPos)) continue;

                var pos = Grid.GridToPixel(gridPos);
                int px = (int)pos.X;
                int py = (int)pos.Y;

                // Draw border around this tile
                // Top edge
                spriteBatch.Draw(_pixelTexture, new Rectangle(px, py, Grid.TileSize, 1), _gridLineColor);
                // Left edge
                spriteBatch.Draw(_pixelTexture, new Rectangle(px, py, 1, Grid.TileSize), _gridLineColor);
                // Bottom edge
                spriteBatch.Draw(_pixelTexture, new Rectangle(px, py + Grid.TileSize, Grid.TileSize + 1, 1), _gridLineColor);
                // Right edge
                spriteBatch.Draw(_pixelTexture, new Rectangle(px + Grid.TileSize, py, 1, Grid.TileSize), _gridLineColor);
            }
        }
    }

    /// <summary>
    /// Draws a colored highlight over a specific tile.
    /// </summary>
    private void DrawTileHighlight(SpriteBatch spriteBatch, Point gridPos, Color color)
    {
        if (!Grid.IsValidPosition(gridPos)) return;

        var pos = Grid.GridToPixel(gridPos);
        spriteBatch.Draw(
            _pixelTexture,
            new Rectangle((int)pos.X, (int)pos.Y, Grid.TileSize, Grid.TileSize),
            color
        );
    }

    /// <summary>
    /// Clean up the pixel texture when done.
    /// </summary>
    public void Dispose()
    {
        _pixelTexture?.Dispose();
    }
}

