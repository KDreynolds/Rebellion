using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegacyOfTheShatteredCrown.Data;
using LegacyOfTheShatteredCrown.Systems;
using LegacyOfTheShatteredCrown.Rendering;

namespace LegacyOfTheShatteredCrown;

/// <summary>
/// Main game class for Legacy of the Shattered Crown.
/// Manages the game loop, coordinates systems, and handles rendering.
/// </summary>
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // Systems
    private InputManager _inputManager = null!;
    private SelectionManager _selectionManager = null!;

    // Rendering
    private GridRenderer _gridRenderer = null!;
    private UnitRenderer _unitRenderer = null!;

    // Game state
    private List<Unit> _units = null!;

    // Screen settings
    private const int ScreenWidth = 720;
    private const int ScreenHeight = 720;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Configure window
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();

        Window.Title = "Legacy of the Shattered Crown - Tactical Grid";

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Initialize renderers
        _gridRenderer = new GridRenderer(GraphicsDevice);
        _unitRenderer = new UnitRenderer(GraphicsDevice);

        // Initialize input
        _inputManager = new InputManager();

        // Create placeholder units for testing
        _units = CreateTestUnits();

        // Initialize selection system
        _selectionManager = new SelectionManager(_units);
    }

    /// <summary>
    /// Creates a set of test units for Milestone 1 demonstration.
    /// Three player heroes and two enemy units.
    /// </summary>
    private List<Unit> CreateTestUnits()
    {
        return new List<Unit>
        {
            // Player heroes (blue team, left side)
            new Unit("Knight", new Point(1, 3), moveRange: 3, new Color(70, 130, 200), isPlayerUnit: true),
            new Unit("Archer", new Point(0, 4), moveRange: 2, new Color(100, 180, 100), isPlayerUnit: true),
            new Unit("Mage", new Point(1, 5), moveRange: 2, new Color(180, 100, 180), isPlayerUnit: true),

            // Enemy units (red team, right side) - not selectable for now
            new Unit("Orc", new Point(6, 2), moveRange: 2, new Color(200, 80, 80), isPlayerUnit: false),
            new Unit("Goblin", new Point(6, 5), moveRange: 3, new Color(180, 120, 60), isPlayerUnit: false)
        };
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit on Escape
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Update input state
        _inputManager.Update();

        // Handle tile clicks
        var clickedTile = _inputManager.GetClickedTile();
        if (clickedTile.HasValue)
        {
            _selectionManager.HandleTileClick(clickedTile.Value);
        }

        // Right-click to deselect
        if (_inputManager.RightClickPressed)
        {
            _selectionManager.Deselect();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Dark background
        GraphicsDevice.Clear(new Color(25, 25, 30));

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        // Draw the grid with highlights
        _gridRenderer.Draw(
            _spriteBatch,
            _inputManager.HoveredTile,
            _selectionManager.SelectedUnit?.GridPosition,
            _selectionManager.ReachableTiles
        );

        // Draw all units
        _unitRenderer.Draw(_spriteBatch, _units, _selectionManager.SelectedUnit);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _gridRenderer?.Dispose();
        _unitRenderer?.Dispose();

        base.UnloadContent();
    }
}

