using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegacyOfTheShatteredCrown.Systems;

namespace LegacyOfTheShatteredCrown.Rendering;

/// <summary>
/// Renders the tactical grid with tile highlights for hover, selection, and movement range.
/// Uses a single-pixel texture for all shape drawing.
/// </summary>
public class GridRenderer
{
    private readonly Texture2D _pixelTexture;

    // Grid colors
    private readonly Color _tileColorLight = new(45, 45, 55);
    private readonly Color _tileColorDark = new(35, 35, 45);
    private readonly Color _gridLineColor = new(60, 60, 70);
    private readonly Color _hoverColor = new(100, 100, 120, 180);
    private readonly Color _selectedColor = new(255, 200, 80, 200);
    private readonly Color _reachableColor = new(80, 180, 120, 150);

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
        List<Point> reachableTiles)
    {
        // Draw base grid tiles
        DrawGridTiles(spriteBatch);

        // Draw reachable tile highlights
        foreach (var tile in reachableTiles)
        {
            DrawTileHighlight(spriteBatch, tile, _reachableColor);
        }

        // Draw selected unit highlight
        if (selectedUnitPosition.HasValue)
        {
            DrawTileHighlight(spriteBatch, selectedUnitPosition.Value, _selectedColor);
        }

        // Draw hover highlight (on top)
        if (hoveredTile.HasValue)
        {
            DrawTileHighlight(spriteBatch, hoveredTile.Value, _hoverColor);
        }

        // Draw grid lines
        DrawGridLines(spriteBatch);
    }

    /// <summary>
    /// Draws the checkerboard pattern of grid tiles.
    /// </summary>
    private void DrawGridTiles(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                var pos = Grid.GridToPixel(new Point(x, y));
                var color = (x + y) % 2 == 0 ? _tileColorLight : _tileColorDark;

                spriteBatch.Draw(
                    _pixelTexture,
                    new Rectangle((int)pos.X, (int)pos.Y, Grid.TileSize, Grid.TileSize),
                    color
                );
            }
        }
    }

    /// <summary>
    /// Draws grid lines for better tile visibility.
    /// </summary>
    private void DrawGridLines(SpriteBatch spriteBatch)
    {
        // Vertical lines
        for (int x = 0; x <= Grid.GridSize; x++)
        {
            var startX = Grid.OffsetX + x * Grid.TileSize;
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(startX, Grid.OffsetY, 1, Grid.TotalHeight),
                _gridLineColor
            );
        }

        // Horizontal lines
        for (int y = 0; y <= Grid.GridSize; y++)
        {
            var startY = Grid.OffsetY + y * Grid.TileSize;
            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(Grid.OffsetX, startY, Grid.TotalWidth, 1),
                _gridLineColor
            );
        }
    }

    /// <summary>
    /// Draws a colored highlight over a specific tile.
    /// </summary>
    private void DrawTileHighlight(SpriteBatch spriteBatch, Point gridPos, Color color)
    {
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

