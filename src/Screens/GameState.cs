namespace LegacyOfTheShatteredCrown.Screens;

/// <summary>
/// Represents the current state/screen of the game.
/// </summary>
public enum GameState
{
    /// <summary>Title screen with Play button.</summary>
    StartScreen,
    /// <summary>Hero selection screen where player picks 3 heroes.</summary>
    HeroSelection,
    /// <summary>Overworld campaign map for province navigation.</summary>
    OverworldMap,
    /// <summary>Active tactical battle.</summary>
    Battle,
    /// <summary>Victory screen after winning a battle.</summary>
    Victory,
    /// <summary>Defeat screen after losing a battle.</summary>
    Defeat
}
