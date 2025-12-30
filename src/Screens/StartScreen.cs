using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LegacyOfTheShatteredCrown.Screens;

/// <summary>
/// The title/start screen of the game.
/// Displays the game title and a Play button.
/// </summary>
public class StartScreen
{
    private readonly Texture2D _pixelTexture;
    private readonly SpriteFont? _font;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private Rectangle _playButtonRect;
    private bool _isPlayButtonHovered;
    private MouseState _previousMouseState;

    /// <summary>
    /// Fired when the player clicks the Play button.
    /// </summary>
    public event Action? OnPlayClicked;

    public StartScreen(GraphicsDevice graphicsDevice, SpriteFont? font, int screenWidth, int screenHeight)
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _font = font;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        // Center the play button
        int buttonWidth = 200;
        int buttonHeight = 60;
        _playButtonRect = new Rectangle(
            (_screenWidth - buttonWidth) / 2,
            _screenHeight / 2 + 50,
            buttonWidth,
            buttonHeight
        );
    }

    public void Update()
    {
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        _isPlayButtonHovered = _playButtonRect.Contains(mousePoint);

        // Check for click
        if (_isPlayButtonHovered &&
            mouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            OnPlayClicked?.Invoke();
        }

        _previousMouseState = mouseState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw background gradient effect (dark to slightly lighter)
        DrawBackground(spriteBatch);

        // Draw title
        DrawTitle(spriteBatch);

        // Draw subtitle
        DrawSubtitle(spriteBatch);

        // Draw play button
        DrawPlayButton(spriteBatch);

        // Draw version/credits
        DrawFooter(spriteBatch);
    }

    private void DrawBackground(SpriteBatch spriteBatch)
    {
        // Simple dark background with subtle pattern
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), new Color(20, 20, 30));

        // Draw some decorative lines
        for (int i = 0; i < 5; i++)
        {
            int y = 100 + i * 120;
            var lineColor = new Color(30, 30, 45);
            spriteBatch.Draw(_pixelTexture, new Rectangle(0, y, _screenWidth, 2), lineColor);
        }
    }

    private void DrawTitle(SpriteBatch spriteBatch)
    {
        string title = "LEGACY OF THE";
        string title2 = "SHATTERED CROWN";

        if (_font != null)
        {
            var titleSize = _font.MeasureString(title);
            var title2Size = _font.MeasureString(title2);

            // Draw shadow
            spriteBatch.DrawString(_font, title,
                new Vector2((_screenWidth - titleSize.X) / 2 + 2, 152),
                new Color(0, 0, 0, 150));
            spriteBatch.DrawString(_font, title2,
                new Vector2((_screenWidth - title2Size.X) / 2 + 2, 192),
                new Color(0, 0, 0, 150));

            // Draw main title
            spriteBatch.DrawString(_font, title,
                new Vector2((_screenWidth - titleSize.X) / 2, 150),
                new Color(220, 180, 100));
            spriteBatch.DrawString(_font, title2,
                new Vector2((_screenWidth - title2Size.X) / 2, 190),
                new Color(255, 220, 120));
        }
        else
        {
            // Fallback: draw decorative boxes for title
            var titleBox = new Rectangle(_screenWidth / 2 - 180, 150, 360, 80);
            spriteBatch.Draw(_pixelTexture, titleBox, new Color(220, 180, 100));

            var innerBox = new Rectangle(titleBox.X + 4, titleBox.Y + 4, titleBox.Width - 8, titleBox.Height - 8);
            spriteBatch.Draw(_pixelTexture, innerBox, new Color(30, 25, 20));
        }
    }

    private void DrawSubtitle(SpriteBatch spriteBatch)
    {
        string subtitle = "A Tactical RPG";

        if (_font != null)
        {
            var subtitleSize = _font.MeasureString(subtitle);
            spriteBatch.DrawString(_font, subtitle,
                new Vector2((_screenWidth - subtitleSize.X) / 2, 250),
                new Color(150, 150, 160));
        }
    }

    private void DrawPlayButton(SpriteBatch spriteBatch)
    {
        // Button colors
        var buttonColor = _isPlayButtonHovered
            ? new Color(100, 160, 100)
            : new Color(70, 130, 70);
        var borderColor = _isPlayButtonHovered
            ? new Color(150, 220, 150)
            : new Color(100, 180, 100);

        // Draw button border
        var borderRect = new Rectangle(
            _playButtonRect.X - 3,
            _playButtonRect.Y - 3,
            _playButtonRect.Width + 6,
            _playButtonRect.Height + 6
        );
        spriteBatch.Draw(_pixelTexture, borderRect, borderColor);

        // Draw button background
        spriteBatch.Draw(_pixelTexture, _playButtonRect, buttonColor);

        // Draw button text
        string buttonText = "PLAY";
        if (_font != null)
        {
            var textSize = _font.MeasureString(buttonText);
            var textPos = new Vector2(
                _playButtonRect.X + (_playButtonRect.Width - textSize.X) / 2,
                _playButtonRect.Y + (_playButtonRect.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(_font, buttonText, textPos, Color.White);
        }
        else
        {
            // Fallback: draw a simple indicator
            var innerRect = new Rectangle(
                _playButtonRect.X + _playButtonRect.Width / 2 - 20,
                _playButtonRect.Y + _playButtonRect.Height / 2 - 10,
                40,
                20
            );
            spriteBatch.Draw(_pixelTexture, innerRect, Color.White);
        }
    }

    private void DrawFooter(SpriteBatch spriteBatch)
    {
        string footer = "Select your heroes and liberate the realm";

        if (_font != null)
        {
            var footerSize = _font.MeasureString(footer);
            spriteBatch.DrawString(_font, footer,
                new Vector2((_screenWidth - footerSize.X) / 2, _screenHeight - 80),
                new Color(100, 100, 110));
        }
    }

    public void Dispose()
    {
        _pixelTexture?.Dispose();
    }
}
