using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Screens;

/// <summary>
/// Screen for selecting 3 heroes before starting a battle.
/// </summary>
public class HeroSelectionScreen
{
    private readonly Texture2D _pixelTexture;
    private readonly SpriteFont? _font;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private readonly List<HeroDefinition> _availableHeroes;
    private readonly List<HeroDefinition> _selectedHeroes;
    private readonly List<Rectangle> _heroCardRects;

    private Rectangle _startBattleButtonRect;
    private bool _isStartButtonHovered;
    private int _hoveredHeroIndex = -1;
    private MouseState _previousMouseState;

    private const int MaxSelectedHeroes = 3;
    private const int CardsPerRow = 4;
    private const int CardWidth = 150;
    private const int CardHeight = 200;
    private const int CardSpacing = 20;

    /// <summary>
    /// Fired when the player clicks Start Battle with 3 heroes selected.
    /// </summary>
    public event Action<List<HeroDefinition>>? OnStartBattle;

    public HeroSelectionScreen(GraphicsDevice graphicsDevice, SpriteFont? font, int screenWidth, int screenHeight)
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _font = font;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _availableHeroes = HeroDefinition.GetAllHeroes();
        _selectedHeroes = new List<HeroDefinition>();
        _heroCardRects = new List<Rectangle>();

        // Calculate card positions
        CalculateCardPositions();

        // Start battle button
        int buttonWidth = 200;
        int buttonHeight = 50;
        _startBattleButtonRect = new Rectangle(
            (_screenWidth - buttonWidth) / 2,
            _screenHeight - 80,
            buttonWidth,
            buttonHeight
        );
    }

    private void CalculateCardPositions()
    {
        _heroCardRects.Clear();

        int totalWidth = CardsPerRow * CardWidth + (CardsPerRow - 1) * CardSpacing;
        int startX = (_screenWidth - totalWidth) / 2;
        int startY = 120;

        for (int i = 0; i < _availableHeroes.Count; i++)
        {
            int row = i / CardsPerRow;
            int col = i % CardsPerRow;

            int x = startX + col * (CardWidth + CardSpacing);
            int y = startY + row * (CardHeight + CardSpacing);

            _heroCardRects.Add(new Rectangle(x, y, CardWidth, CardHeight));
        }
    }

    public void Update()
    {
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Check hero card hovers
        _hoveredHeroIndex = -1;
        for (int i = 0; i < _heroCardRects.Count; i++)
        {
            if (_heroCardRects[i].Contains(mousePoint))
            {
                _hoveredHeroIndex = i;
                break;
            }
        }

        // Check start button hover
        _isStartButtonHovered = _startBattleButtonRect.Contains(mousePoint) && _selectedHeroes.Count == MaxSelectedHeroes;

        // Handle clicks
        if (mouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            // Hero card click
            if (_hoveredHeroIndex >= 0 && _hoveredHeroIndex < _availableHeroes.Count)
            {
                var hero = _availableHeroes[_hoveredHeroIndex];
                ToggleHeroSelection(hero);
            }

            // Start battle click
            if (_isStartButtonHovered && _selectedHeroes.Count == MaxSelectedHeroes)
            {
                OnStartBattle?.Invoke(new List<HeroDefinition>(_selectedHeroes));
            }
        }

        _previousMouseState = mouseState;
    }

    private void ToggleHeroSelection(HeroDefinition hero)
    {
        if (_selectedHeroes.Contains(hero))
        {
            _selectedHeroes.Remove(hero);
        }
        else if (_selectedHeroes.Count < MaxSelectedHeroes)
        {
            _selectedHeroes.Add(hero);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), new Color(25, 25, 35));

        // Title
        DrawTitle(spriteBatch);

        // Selection count
        DrawSelectionCount(spriteBatch);

        // Hero cards
        for (int i = 0; i < _availableHeroes.Count; i++)
        {
            DrawHeroCard(spriteBatch, i);
        }

        // Selected heroes preview
        DrawSelectedHeroesPreview(spriteBatch);

        // Start battle button
        DrawStartBattleButton(spriteBatch);
    }

    private void DrawTitle(SpriteBatch spriteBatch)
    {
        string title = "SELECT YOUR HEROES";

        if (_font != null)
        {
            var titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title,
                new Vector2((_screenWidth - titleSize.X) / 2, 30),
                new Color(220, 180, 100));
        }
        else
        {
            var titleBar = new Rectangle(_screenWidth / 2 - 150, 30, 300, 40);
            spriteBatch.Draw(_pixelTexture, titleBar, new Color(220, 180, 100));
        }
    }

    private void DrawSelectionCount(SpriteBatch spriteBatch)
    {
        string count = $"Selected: {_selectedHeroes.Count} / {MaxSelectedHeroes}";

        if (_font != null)
        {
            var countSize = _font.MeasureString(count);
            var countColor = _selectedHeroes.Count == MaxSelectedHeroes
                ? new Color(100, 200, 100)
                : new Color(180, 180, 180);
            spriteBatch.DrawString(_font, count,
                new Vector2((_screenWidth - countSize.X) / 2, 70),
                countColor);
        }
    }

    private void DrawHeroCard(SpriteBatch spriteBatch, int index)
    {
        var hero = _availableHeroes[index];
        var rect = _heroCardRects[index];
        bool isSelected = _selectedHeroes.Contains(hero);
        bool isHovered = _hoveredHeroIndex == index;

        // Card border
        var borderColor = isSelected
            ? new Color(100, 200, 100)
            : isHovered
                ? new Color(200, 200, 100)
                : new Color(60, 60, 70);

        var borderRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
        spriteBatch.Draw(_pixelTexture, borderRect, borderColor);

        // Card background
        var bgColor = isSelected
            ? new Color(40, 60, 40)
            : new Color(35, 35, 45);
        spriteBatch.Draw(_pixelTexture, rect, bgColor);

        // Hero color indicator (avatar placeholder)
        var avatarRect = new Rectangle(rect.X + 10, rect.Y + 10, rect.Width - 20, 60);
        spriteBatch.Draw(_pixelTexture, avatarRect, hero.Color);

        // Hero name
        if (_font != null)
        {
            // Truncate long names
            string displayName = hero.Name.Length > 14 ? hero.Name.Substring(0, 12) + ".." : hero.Name;
            var nameSize = _font.MeasureString(displayName);
            float nameX = rect.X + (rect.Width - nameSize.X) / 2;
            spriteBatch.DrawString(_font, displayName,
                new Vector2(nameX, rect.Y + 80),
                Color.White);
        }

        // Stats display
        DrawHeroStats(spriteBatch, hero, rect);

        // House affiliation
        if (_font != null)
        {
            string house = hero.HouseAffiliation.Replace("House ", "");
            if (house.Length > 10) house = house.Substring(0, 8) + "..";
            var houseSize = _font.MeasureString(house);
            float houseX = rect.X + (rect.Width - houseSize.X) / 2;
            spriteBatch.DrawString(_font, house,
                new Vector2(houseX, rect.Y + rect.Height - 25),
                new Color(150, 150, 160));
        }

        // Selection indicator
        if (isSelected)
        {
            int selectionNumber = _selectedHeroes.IndexOf(hero) + 1;
            var indicatorRect = new Rectangle(rect.X + rect.Width - 25, rect.Y + 5, 20, 20);
            spriteBatch.Draw(_pixelTexture, indicatorRect, new Color(100, 200, 100));

            if (_font != null)
            {
                spriteBatch.DrawString(_font, selectionNumber.ToString(),
                    new Vector2(indicatorRect.X + 6, indicatorRect.Y + 2),
                    Color.Black);
            }
        }
    }

    private void DrawHeroStats(SpriteBatch spriteBatch, HeroDefinition hero, Rectangle cardRect)
    {
        int startY = cardRect.Y + 105;
        int leftX = cardRect.X + 10;
        int rightX = cardRect.X + cardRect.Width / 2 + 5;
        int lineHeight = 18;

        if (_font != null)
        {
            // HP
            spriteBatch.DrawString(_font, $"HP:{hero.BaseMaxHP}",
                new Vector2(leftX, startY),
                new Color(200, 100, 100));

            // Move
            spriteBatch.DrawString(_font, $"MV:{hero.BaseMoveRange}",
                new Vector2(rightX, startY),
                new Color(100, 180, 220));

            // Attack
            spriteBatch.DrawString(_font, $"ATK:{hero.BaseAttackPower}",
                new Vector2(leftX, startY + lineHeight),
                new Color(220, 180, 100));

            // Defense
            spriteBatch.DrawString(_font, $"DEF:{hero.BaseDefense}",
                new Vector2(rightX, startY + lineHeight),
                new Color(100, 200, 100));

            // Range
            string rangeText = hero.BaseAttackRange > 1 ? $"RNG:{hero.BaseAttackRange}" : "Melee";
            spriteBatch.DrawString(_font, rangeText,
                new Vector2(leftX, startY + lineHeight * 2),
                new Color(180, 150, 220));
        }
    }

    private void DrawSelectedHeroesPreview(SpriteBatch spriteBatch)
    {
        int previewY = _screenHeight - 140;
        int previewSize = 40;
        int spacing = 10;
        int startX = 20;

        // Label
        if (_font != null)
        {
            spriteBatch.DrawString(_font, "Team:",
                new Vector2(startX, previewY - 25),
                new Color(180, 180, 180));
        }

        // Draw selected hero previews
        for (int i = 0; i < MaxSelectedHeroes; i++)
        {
            var previewRect = new Rectangle(
                startX + i * (previewSize + spacing),
                previewY,
                previewSize,
                previewSize
            );

            if (i < _selectedHeroes.Count)
            {
                // Filled slot
                var hero = _selectedHeroes[i];
                spriteBatch.Draw(_pixelTexture, previewRect, hero.Color);

                // Border
                DrawRectangleBorder(spriteBatch, previewRect, new Color(100, 200, 100), 2);
            }
            else
            {
                // Empty slot
                spriteBatch.Draw(_pixelTexture, previewRect, new Color(40, 40, 50));
                DrawRectangleBorder(spriteBatch, previewRect, new Color(60, 60, 70), 2);
            }
        }
    }

    private void DrawStartBattleButton(SpriteBatch spriteBatch)
    {
        bool canStart = _selectedHeroes.Count == MaxSelectedHeroes;

        var buttonColor = !canStart
            ? new Color(60, 60, 60)
            : _isStartButtonHovered
                ? new Color(100, 160, 100)
                : new Color(70, 130, 70);

        var borderColor = !canStart
            ? new Color(80, 80, 80)
            : _isStartButtonHovered
                ? new Color(150, 220, 150)
                : new Color(100, 180, 100);

        // Border
        var borderRect = new Rectangle(
            _startBattleButtonRect.X - 3,
            _startBattleButtonRect.Y - 3,
            _startBattleButtonRect.Width + 6,
            _startBattleButtonRect.Height + 6
        );
        spriteBatch.Draw(_pixelTexture, borderRect, borderColor);

        // Button
        spriteBatch.Draw(_pixelTexture, _startBattleButtonRect, buttonColor);

        // Text
        string buttonText = canStart ? "START BATTLE" : "Select 3 Heroes";
        if (_font != null)
        {
            var textSize = _font.MeasureString(buttonText);
            var textPos = new Vector2(
                _startBattleButtonRect.X + (_startBattleButtonRect.Width - textSize.X) / 2,
                _startBattleButtonRect.Y + (_startBattleButtonRect.Height - textSize.Y) / 2
            );
            var textColor = canStart ? Color.White : new Color(120, 120, 120);
            spriteBatch.DrawString(_font, buttonText, textPos, textColor);
        }
    }

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

    public void Reset()
    {
        _selectedHeroes.Clear();
    }

    public void Dispose()
    {
        _pixelTexture?.Dispose();
    }
}
