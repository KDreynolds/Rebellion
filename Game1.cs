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
    private OverworldMapScreen _overworldMapScreen = null!;

    // Battle Systems
    private InputManager _inputManager = null!;
    private SelectionManager _selectionManager = null!;
    private CombatSystem _combatSystem = null!;
    private TurnManager _turnManager = null!;
    private EnemyAI _enemyAI = null!;

    // Input tracking for turn end
    private KeyboardState _previousKeyboardState;

    // Rendering
    private GridRenderer _gridRenderer = null!;
    private UnitRenderer _unitRenderer = null!;

    // Game state
    private List<Unit> _units = null!;
    private List<HeroDefinition> _selectedHeroes = null!;
    private Province? _currentBattleProvince;

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
        _heroSelectionScreen.OnStartBattle += OnHeroesSelected;

        _overworldMapScreen = new OverworldMapScreen(GraphicsDevice, _font, ScreenWidth, ScreenHeight);
        _overworldMapScreen.OnBattleStart += OnBattleStart;

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

    private void OnHeroesSelected(List<HeroDefinition> selectedHeroes)
    {
        _selectedHeroes = selectedHeroes;
        _currentState = GameState.OverworldMap;
    }

    private void OnBattleStart(Province province)
    {
        _currentBattleProvince = province;
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

        // Determine enemy composition based on province difficulty
        int difficulty = _currentBattleProvince?.Difficulty ?? 1;
        SpawnEnemiesForDifficulty(difficulty);

        // Initialize combat and turn systems
        _combatSystem = new CombatSystem(_units);
        _turnManager = new TurnManager(_units);
        _enemyAI = new EnemyAI(_units, _combatSystem);

        // Initialize selection system with new units and wire combat
        _selectionManager = new SelectionManager(_units);
        _selectionManager.SetCombatSystem(_combatSystem);

        // Subscribe to combat events
        _selectionManager.OnUnitDefeated += OnUnitDefeated;
        _enemyAI.OnEnemyAttack += OnEnemyAttack;
    }

    private void OnEnemyAttack(Unit attacker, Unit target, int damage)
    {
        // Could add visual feedback here later (damage numbers, flash, etc.)
        _turnManager.RefreshUnitLists();
    }

    private void OnUnitDefeated(Unit unit)
    {
        // Refresh the turn manager's unit lists
        _turnManager.RefreshUnitLists();
    }

    private void SpawnEnemiesForDifficulty(int difficulty)
    {
        // Base enemy positions
        Point[] enemyPositions = { new Point(6, 2), new Point(6, 5), new Point(7, 4), new Point(7, 3), new Point(6, 4) };

        // Difficulty 1: 2 weak enemies
        // Difficulty 2: 3 medium enemies
        // Difficulty 3: 3-4 strong enemies
        // Difficulty 4+: 4+ tough enemies

        int enemyCount = Math.Min(difficulty + 1, enemyPositions.Length);
        float statMultiplier = 1.0f + (difficulty - 1) * 0.25f;

        for (int i = 0; i < enemyCount; i++)
        {
            var (name, hp, atk, def, move, color) = GetEnemyTemplate(i, difficulty);

            _units.Add(new Unit(
                name: name,
                gridPosition: enemyPositions[i],
                moveRange: move,
                color: color,
                maxHP: (int)(hp * statMultiplier),
                attackRange: 1,
                attackPower: (int)(atk * statMultiplier),
                defense: def,
                isPlayerUnit: false
            ));
        }
    }

    private (string name, int hp, int atk, int def, int move, Color color) GetEnemyTemplate(int index, int difficulty)
    {
        // Rotate through enemy types
        return (index % 4, difficulty) switch
        {
            (0, >= 4) => ("Orc Warlord", 16, 6, 2, 2, new Color(160, 40, 40)),
            (0, _) => ("Orc Warrior", 10, 4, 1, 2, new Color(200, 80, 80)),
            (1, >= 3) => ("Orc Archer", 8, 5, 0, 2, new Color(180, 100, 80)),
            (1, _) => ("Goblin Scout", 6, 2, 0, 3, new Color(180, 120, 60)),
            (2, >= 4) => ("Orc Champion", 14, 5, 1, 2, new Color(180, 60, 60)),
            (2, _) => ("Orc Berserker", 12, 5, 0, 2, new Color(180, 60, 60)),
            (3, _) => ("Goblin Shaman", 7, 3, 0, 2, new Color(120, 160, 80)),
            _ => ("Orc Grunt", 8, 3, 0, 2, new Color(190, 90, 70))
        };
    }

    private void CheckBattleEnd()
    {
        bool playerAlive = _units.Any(u => u.IsPlayerUnit && u.IsAlive);
        bool enemyAlive = _units.Any(u => !u.IsPlayerUnit && u.IsAlive);

        if (!enemyAlive && playerAlive)
        {
            // Victory!
            OnBattleVictory();
        }
        else if (!playerAlive)
        {
            // Defeat - return to map (could show defeat screen)
            _currentState = GameState.OverworldMap;
            _currentBattleProvince = null;
        }
    }

    private void OnBattleVictory()
    {
        if (_currentBattleProvince != null)
        {
            _overworldMapScreen.OnBattleWon(_currentBattleProvince.Id);
            _currentBattleProvince = null;
        }
        _currentState = GameState.OverworldMap;
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

            case GameState.OverworldMap:
                _overworldMapScreen.Update();
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
            case GameState.OverworldMap:
                _currentState = GameState.HeroSelection;
                _heroSelectionScreen.Reset();
                break;
            case GameState.Battle:
                // Return to overworld map (retreat from battle)
                _currentState = GameState.OverworldMap;
                _currentBattleProvince = null;
                break;
        }
    }

    private void UpdateBattle()
    {
        var currentKeyboardState = Keyboard.GetState();

        // Update input state
        _inputManager.Update();

        // Handle player turn
        if (_turnManager.IsPlayerTurn && !_turnManager.IsProcessing)
        {
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

            // Space or Enter to end player turn
            bool endTurnPressed = (currentKeyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space)) ||
                                  (currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter));

            if (endTurnPressed)
            {
                EndPlayerTurn();
            }
        }

        // Handle enemy turn
        if (_turnManager.IsEnemyTurn && _turnManager.IsProcessing)
        {
            ProcessEnemyTurn();
        }

        _previousKeyboardState = currentKeyboardState;

        // Check for battle end conditions
        CheckBattleEnd();
    }

    private void EndPlayerTurn()
    {
        _selectionManager.Deselect();
        _turnManager.EndTurn(); // Switches to enemy turn
    }

    private void ProcessEnemyTurn()
    {
        // Execute all enemy actions
        _enemyAI.ExecuteEnemyTurn();

        // Complete the enemy turn (switches back to player)
        _turnManager.CompleteEnemyTurn();
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

            case GameState.OverworldMap:
                _overworldMapScreen.Draw(_spriteBatch);
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
        // Draw the grid with highlights (movement and attack ranges)
        _gridRenderer.Draw(
            _spriteBatch,
            _inputManager.HoveredTile,
            _selectionManager.SelectedUnit?.GridPosition,
            _selectionManager.ReachableTiles,
            _selectionManager.AttackableTiles
        );

        // Draw all units (only alive ones)
        var aliveUnits = _units.Where(u => u.IsAlive).ToList();
        _unitRenderer.Draw(_spriteBatch, aliveUnits, _selectionManager.SelectedUnit);

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

        // Draw province name if in campaign battle
        if (_currentBattleProvince != null)
        {
            string provinceInfo = $"Battle: {_currentBattleProvince.Name}";
            var provinceSize = _font.MeasureString(provinceInfo);
            _spriteBatch.DrawString(_font, provinceInfo,
                new Vector2(ScreenWidth - provinceSize.X - 10, 10),
                new Color(220, 180, 100));
        }

        // Draw turn indicator
        string turnText = _turnManager.IsPlayerTurn ? "YOUR TURN" : "ENEMY TURN";
        var turnColor = _turnManager.IsPlayerTurn ? new Color(100, 200, 100) : new Color(200, 100, 100);
        var turnSize = _font.MeasureString(turnText);
        _spriteBatch.DrawString(_font, turnText,
            new Vector2((ScreenWidth - turnSize.X) / 2, ScreenHeight - 55),
            turnColor);

        // Draw control hints
        _spriteBatch.DrawString(_font, "SPACE: End Turn | ESC: Return to Map",
            new Vector2(10, ScreenHeight - 30), new Color(100, 100, 110));
    }

    protected override void UnloadContent()
    {
        _startScreen?.Dispose();
        _heroSelectionScreen?.Dispose();
        _overworldMapScreen?.Dispose();
        _gridRenderer?.Dispose();
        _unitRenderer?.Dispose();

        base.UnloadContent();
    }
}
