using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegacyOfTheShatteredCrown.Data;
using LegacyOfTheShatteredCrown.Systems;
using LegacyOfTheShatteredCrown.Rendering;
using LegacyOfTheShatteredCrown.Screens;

namespace LegacyOfTheShatteredCrown;

/// <summary>
/// Main game class for Legacy of the Shattered Crown.
/// Manages the game loop, coordinates systems, and handles rendering.
/// </summary>
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private SpriteFont? _font;

    // Current game state
    private GameState _currentState = GameState.StartScreen;

    // Screens
    private StartScreen _startScreen = null!;
    private HeroSelectionScreen _heroSelectionScreen = null!;

    // Battle Systems
    private InputManager _inputManager = null!;
    private SelectionManager _selectionManager = null!;

    // Rendering
    private GridRenderer _gridRenderer = null!;
    private UnitRenderer _unitRenderer = null!;

    // Game state
    private List<Unit> _units = null!;
    private List<HeroDefinition> _selectedHeroes = null!;

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

        Window.Title = "Legacy of the Shattered Crown";

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Try to load font (may not exist yet)
        try
        {
            _font = Content.Load<SpriteFont>("DefaultFont");
        }
        catch
        {
            // Font not available, screens will use fallback rendering
            _font = null;
        }

        // Initialize screens
        _startScreen = new StartScreen(GraphicsDevice, _font, ScreenWidth, ScreenHeight);
        _startScreen.OnPlayClicked += OnPlayClicked;

        _heroSelectionScreen = new HeroSelectionScreen(GraphicsDevice, _font, ScreenWidth, ScreenHeight);
        _heroSelectionScreen.OnStartBattle += OnStartBattle;

        // Initialize battle renderers (created once, reused)
        _gridRenderer = new GridRenderer(GraphicsDevice);
        _unitRenderer = new UnitRenderer(GraphicsDevice);

        // Initialize input
        _inputManager = new InputManager();

        // Initialize empty units list
        _units = new List<Unit>();
    }

    private void OnPlayClicked()
    {
        _currentState = GameState.HeroSelection;
        _heroSelectionScreen.Reset();
    }

    private void OnStartBattle(List<HeroDefinition> selectedHeroes)
    {
        _selectedHeroes = selectedHeroes;
        _currentState = GameState.Battle;
        InitializeBattle();
    }

    private void InitializeBattle()
    {
        _units = new List<Unit>();

        // Create player heroes from selection
        Point[] playerPositions = { new Point(1, 3), new Point(0, 4), new Point(1, 5) };
        for (int i = 0; i < _selectedHeroes.Count && i < playerPositions.Length; i++)
        {
            _units.Add(_selectedHeroes[i].CreateUnit(playerPositions[i]));
        }

        // Add enemy units
        _units.Add(new Unit(
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

        _units.Add(new Unit(
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

        _units.Add(new Unit(
            name: "Orc Berserker",
            gridPosition: new Point(7, 4),
            moveRange: 2,
            color: new Color(180, 60, 60),
            maxHP: 12,
            attackRange: 1,
            attackPower: 5,
            defense: 0,
            isPlayerUnit: false
        ));

        // Initialize selection system with new units
        _selectionManager = new SelectionManager(_units);
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit on Escape (returns to previous screen or exits)
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            HandleEscape();
        }

        switch (_currentState)
        {
            case GameState.StartScreen:
                _startScreen.Update();
                break;

            case GameState.HeroSelection:
                _heroSelectionScreen.Update();
                break;

            case GameState.Battle:
                UpdateBattle();
                break;
        }

        base.Update(gameTime);
    }

    private void HandleEscape()
    {
        switch (_currentState)
        {
            case GameState.StartScreen:
                Exit();
                break;
            case GameState.HeroSelection:
                _currentState = GameState.StartScreen;
                break;
            case GameState.Battle:
                // For now, return to hero selection
                _currentState = GameState.HeroSelection;
                _heroSelectionScreen.Reset();
                break;
        }
    }

    private void UpdateBattle()
    {
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
    }

    protected override void Draw(GameTime gameTime)
    {
        // Dark background
        GraphicsDevice.Clear(new Color(25, 25, 30));

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        switch (_currentState)
        {
            case GameState.StartScreen:
                _startScreen.Draw(_spriteBatch);
                break;

            case GameState.HeroSelection:
                _heroSelectionScreen.Draw(_spriteBatch);
                break;

            case GameState.Battle:
                DrawBattle();
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawBattle()
    {
        // Draw the grid with highlights
        _gridRenderer.Draw(
            _spriteBatch,
            _inputManager.HoveredTile,
            _selectionManager.SelectedUnit?.GridPosition,
            _selectionManager.ReachableTiles
        );

        // Draw all units
        _unitRenderer.Draw(_spriteBatch, _units, _selectionManager.SelectedUnit);

        // Draw battle UI hints
        DrawBattleUI();
    }

    private void DrawBattleUI()
    {
        if (_font == null) return;

        // Draw selected unit info
        if (_selectionManager.SelectedUnit != null)
        {
            var unit = _selectionManager.SelectedUnit;
            string info = $"{unit.Name} - HP: {unit.CurrentHP}/{unit.MaxHP}";
            _spriteBatch.DrawString(_font, info, new Vector2(10, 10), Color.White);

            // Draw abilities
            int abilityY = 35;
            for (int i = 0; i < unit.Abilities.Count; i++)
            {
                var ability = unit.Abilities[i];
                string abilityText = ability.IsReady
                    ? $"[{i + 1}] {ability.Name}"
                    : $"[{i + 1}] {ability.Name} (CD: {ability.CurrentCooldown})";
                var abilityColor = ability.IsReady ? new Color(100, 200, 100) : new Color(150, 150, 150);
                _spriteBatch.DrawString(_font, abilityText, new Vector2(10, abilityY), abilityColor);
                abilityY += 20;
            }

            // Draw passive trait
            if (unit.PassiveTrait != null)
            {
                string passiveText = $"Passive: {unit.PassiveTrait.Name}";
                _spriteBatch.DrawString(_font, passiveText, new Vector2(10, abilityY + 5), new Color(180, 150, 220));
            }
        }

        // Draw ESC hint
        _spriteBatch.DrawString(_font, "ESC: Return to Hero Select",
            new Vector2(10, ScreenHeight - 30), new Color(100, 100, 110));
    }

    protected override void UnloadContent()
    {
        _startScreen?.Dispose();
        _heroSelectionScreen?.Dispose();
        _gridRenderer?.Dispose();
        _unitRenderer?.Dispose();

        base.UnloadContent();
    }
}
