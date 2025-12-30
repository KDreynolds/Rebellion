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

        // Create hero units using definitions
        _units = CreateHeroUnits();

        // Initialize selection system
        _selectionManager = new SelectionManager(_units);
    }

    /// <summary>
    /// Creates hero units using HeroDefinition templates.
    /// Three player heroes and two enemy units.
    /// </summary>
    private List<Unit> CreateHeroUnits()
    {
        var units = new List<Unit>();

        // Player heroes (using HeroDefinitions)
        units.Add(HeroDefinition.SirAldric().CreateUnit(new Point(1, 3)));
        units.Add(HeroDefinition.LyraSwiftbow().CreateUnit(new Point(0, 4)));
        units.Add(HeroDefinition.MiraFlamecaller().CreateUnit(new Point(1, 5)));

        // Enemy units (basic enemies for now)
        units.Add(new Unit(
            name: "Orc Warrior",
            gridPosition: new Point(6, 2),
            moveRange: 2,
            color: new Color(200, 80, 80),
            maxHP: 10,
            attackRange: 1,
            attackPower: 4,
            defense: 1,
            isPlayerUnit: false
        ));

        units.Add(new Unit(
            name: "Goblin Scout",
            gridPosition: new Point(6, 5),
            moveRange: 3,
            color: new Color(180, 120, 60),
            maxHP: 6,
            attackRange: 1,
            attackPower: 2,
            defense: 0,
            isPlayerUnit: false
        ));

        return units;
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
