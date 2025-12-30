using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Represents the current state of a province.
/// </summary>
public enum ProvinceState
{
    /// <summary>Province is held by enemy forces.</summary>
    Hostile,
    /// <summary>Province has been liberated by the player.</summary>
    Liberated,
    /// <summary>Province where the player currently is.</summary>
    Current
}

/// <summary>
/// Represents a single province node on the overworld map.
/// Provinces belong to realms and can be liberated through battle.
/// </summary>
public class Province
{
    /// <summary>
    /// Unique identifier for this province.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Display name of the province.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The realm this province belongs to.
    /// </summary>
    public string RealmId { get; }

    /// <summary>
    /// Position on the map for rendering (normalized 0-1 coordinates).
    /// </summary>
    public Vector2 MapPosition { get; }

    /// <summary>
    /// IDs of provinces adjacent to this one (player can travel between adjacent provinces).
    /// </summary>
    public List<string> AdjacentProvinceIds { get; }

    /// <summary>
    /// Current state of the province.
    /// </summary>
    public ProvinceState State { get; set; }

    /// <summary>
    /// Difficulty rating of the battle (affects enemy count/strength).
    /// </summary>
    public int Difficulty { get; }

    /// <summary>
    /// Description or flavor text for the province.
    /// </summary>
    public string Description { get; }

    public Province(
        string id,
        string name,
        string realmId,
        Vector2 mapPosition,
        List<string> adjacentProvinceIds,
        int difficulty = 1,
        string description = "",
        ProvinceState state = ProvinceState.Hostile)
    {
        Id = id;
        Name = name;
        RealmId = realmId;
        MapPosition = mapPosition;
        AdjacentProvinceIds = adjacentProvinceIds;
        Difficulty = difficulty;
        Description = description;
        State = state;
    }

    /// <summary>
    /// Checks if this province is adjacent to another.
    /// </summary>
    public bool IsAdjacentTo(string provinceId)
    {
        return AdjacentProvinceIds.Contains(provinceId);
    }
}
