using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Represents an Ancient House's realm - a collection of provinces.
/// When all provinces in a realm are liberated, the House is restored.
/// </summary>
public class Realm
{
    /// <summary>
    /// Unique identifier for this realm.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Display name of the realm.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The Ancient House that rules this realm.
    /// </summary>
    public string HouseAffiliation { get; }

    /// <summary>
    /// Color associated with this realm/house.
    /// </summary>
    public Color Color { get; }

    /// <summary>
    /// IDs of provinces that belong to this realm.
    /// </summary>
    public List<string> ProvinceIds { get; }

    /// <summary>
    /// Description of the realm buff granted when liberated.
    /// </summary>
    public string RealmBuffDescription { get; }

    public Realm(
        string id,
        string name,
        string houseAffiliation,
        Color color,
        List<string> provinceIds,
        string realmBuffDescription = "")
    {
        Id = id;
        Name = name;
        HouseAffiliation = houseAffiliation;
        Color = color;
        ProvinceIds = provinceIds;
        RealmBuffDescription = realmBuffDescription;
    }
}
