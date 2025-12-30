using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegacyOfTheShatteredCrown.Data;
using LegacyOfTheShatteredCrown.Systems;

namespace LegacyOfTheShatteredCrown.Rendering;

/// <summary>
/// Renders units on the tactical grid as colored rectangles with HP bars.
/// Will be expanded later to support sprites and animations.
/// </summary>
public class UnitRenderer
{
    private readonly Texture2D _pixelTexture;
    private readonly int _unitSize = 48; // Slightly smaller than tile
    private readonly int _unitPadding = 8; // Padding from tile edge
    private readonly int _hpBarHeight = 6;
    private readonly int _hpBarPadding = 2;

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
            if (unit.IsAlive)
            {
                DrawUnit(spriteBatch, unit, unit == selectedUnit);
            }
        }
    }

    /// <summary>
    /// Draws a single unit as a colored rectangle with a border and HP bar.
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

        // Draw HP bar below the unit
        DrawHPBar(spriteBatch, unit, unitRect);

        // Draw ability cooldown indicators if unit has abilities
        if (unit.Abilities.Count > 0)
        {
            DrawAbilityIndicators(spriteBatch, unit, unitRect);
        }
    }

    /// <summary>
    /// Draws an HP bar below the unit.
    /// </summary>
    private void DrawHPBar(SpriteBatch spriteBatch, Unit unit, Rectangle unitRect)
    {
        // HP bar background (dark gray)
        var hpBarBg = new Rectangle(
            unitRect.X,
            unitRect.Y + unitRect.Height + _hpBarPadding,
            unitRect.Width,
            _hpBarHeight
        );
        spriteBatch.Draw(_pixelTexture, hpBarBg, new Color(40, 40, 40));

        // HP bar border
        var hpBarBorder = new Rectangle(
            hpBarBg.X - 1,
            hpBarBg.Y - 1,
            hpBarBg.Width + 2,
            hpBarBg.Height + 2
        );
        DrawRectangleBorder(spriteBatch, hpBarBorder, new Color(60, 60, 60), 1);

        // Calculate HP bar fill width
        int fillWidth = (int)(unitRect.Width * unit.HPPercentage);

        if (fillWidth > 0)
        {
            // HP bar fill (color based on HP percentage)
            var hpBarFill = new Rectangle(
                unitRect.X,
                unitRect.Y + unitRect.Height + _hpBarPadding,
                fillWidth,
                _hpBarHeight
            );
            var hpColor = GetHPColor(unit.HPPercentage);
            spriteBatch.Draw(_pixelTexture, hpBarFill, hpColor);
        }
    }

    /// <summary>
    /// Draws small indicators for ability cooldowns.
    /// </summary>
    private void DrawAbilityIndicators(SpriteBatch spriteBatch, Unit unit, Rectangle unitRect)
    {
        int indicatorSize = 6;
        int spacing = 2;
        int startX = unitRect.X;
        int y = unitRect.Y - indicatorSize - spacing;

        for (int i = 0; i < unit.Abilities.Count && i < 4; i++) // Max 4 indicators
        {
            var ability = unit.Abilities[i];
            var indicatorRect = new Rectangle(
                startX + i * (indicatorSize + spacing),
                y,
                indicatorSize,
                indicatorSize
            );

            // Green if ready, orange/red if on cooldown
            Color color = ability.IsReady
                ? new Color(100, 200, 100)
                : new Color(200, 100, 50);

            spriteBatch.Draw(_pixelTexture, indicatorRect, color);
        }
    }

    /// <summary>
    /// Gets HP bar color based on HP percentage.
    /// Green (>60%), Yellow (30-60%), Red (<30%).
    /// </summary>
    private Color GetHPColor(float percentage)
    {
        if (percentage > 0.6f)
        {
            return new Color(80, 200, 80); // Green
        }
        else if (percentage > 0.3f)
        {
            return new Color(220, 180, 50); // Yellow
        }
        else
        {
            return new Color(200, 60, 60); // Red
        }
    }

    /// <summary>
    /// Draws a rectangle border (outline only).
    /// </summary>
    private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

    /// <summary>
    /// Clean up the pixel texture when done.
    /// </summary>
    public void Dispose()
    {
        _pixelTexture?.Dispose();
    }
}
