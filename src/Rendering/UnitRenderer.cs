using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegacyOfTheShatteredCrown.Data;
using LegacyOfTheShatteredCrown.Systems;

namespace LegacyOfTheShatteredCrown.Rendering;

/// <summary>
/// Renders units on the tactical grid as colored circles.
/// Will be expanded later to support sprites and animations.
/// </summary>
public class UnitRenderer
{
    private readonly Texture2D _pixelTexture;
    private readonly int _unitSize = 48; // Slightly smaller than tile
    private readonly int _unitPadding = 8; // Padding from tile edge

    public UnitRenderer(GraphicsDevice graphicsDevice)
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    /// <summary>
    /// Draws all units in the provided list.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, List<Unit> units, Unit? selectedUnit)
    {
        foreach (var unit in units)
        {
            DrawUnit(spriteBatch, unit, unit == selectedUnit);
        }
    }

    /// <summary>
    /// Draws a single unit as a colored rectangle with a border.
    /// </summary>
    private void DrawUnit(SpriteBatch spriteBatch, Unit unit, bool isSelected)
    {
        var pixelPos = Grid.GridToPixel(unit.GridPosition);

        // Calculate unit rectangle (centered in tile)
        var unitRect = new Rectangle(
            (int)pixelPos.X + _unitPadding,
            (int)pixelPos.Y + _unitPadding,
            _unitSize,
            _unitSize
        );

        // Draw selection border if selected
        if (isSelected)
        {
            var borderRect = new Rectangle(
                unitRect.X - 3,
                unitRect.Y - 3,
                unitRect.Width + 6,
                unitRect.Height + 6
            );
            spriteBatch.Draw(_pixelTexture, borderRect, new Color(255, 220, 100));
        }

        // Draw unit background (darker shade)
        var bgColor = new Color(
            (int)(unit.Color.R * 0.5f),
            (int)(unit.Color.G * 0.5f),
            (int)(unit.Color.B * 0.5f)
        );
        spriteBatch.Draw(_pixelTexture, unitRect, bgColor);

        // Draw unit main body (slightly smaller for border effect)
        var innerRect = new Rectangle(
            unitRect.X + 2,
            unitRect.Y + 2,
            unitRect.Width - 4,
            unitRect.Height - 4
        );
        spriteBatch.Draw(_pixelTexture, innerRect, unit.Color);

        // Draw a small indicator in the corner for player vs enemy
        var indicatorSize = 8;
        var indicatorRect = new Rectangle(
            unitRect.X + unitRect.Width - indicatorSize - 2,
            unitRect.Y + 2,
            indicatorSize,
            indicatorSize
        );
        var indicatorColor = unit.IsPlayerUnit ? new Color(100, 200, 100) : new Color(200, 80, 80);
        spriteBatch.Draw(_pixelTexture, indicatorRect, indicatorColor);
    }

    /// <summary>
    /// Clean up the pixel texture when done.
    /// </summary>
    public void Dispose()
    {
        _pixelTexture?.Dispose();
    }
}

