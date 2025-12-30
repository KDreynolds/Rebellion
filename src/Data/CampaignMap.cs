using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Contains the full campaign map data including all provinces and realms.
/// Also manages campaign state (current province, liberation status).
/// </summary>
public class CampaignMap
{
    /// <summary>
    /// All provinces in the campaign.
    /// </summary>
    public Dictionary<string, Province> Provinces { get; }

    /// <summary>
    /// All realms in the campaign.
    /// </summary>
    public Dictionary<string, Realm> Realms { get; }

    /// <summary>
    /// ID of the province where the player currently is.
    /// </summary>
    public string CurrentProvinceId { get; private set; }

    public CampaignMap()
    {
        Provinces = new Dictionary<string, Province>();
        Realms = new Dictionary<string, Realm>();
        CurrentProvinceId = "";
    }

    /// <summary>
    /// Gets the current province.
    /// </summary>
    public Province? CurrentProvince =>
        Provinces.TryGetValue(CurrentProvinceId, out var province) ? province : null;

    /// <summary>
    /// Sets the current province and updates states.
    /// </summary>
    public void SetCurrentProvince(string provinceId)
    {
        // Clear previous current state
        if (CurrentProvince != null && CurrentProvince.State == ProvinceState.Current)
        {
            CurrentProvince.State = ProvinceState.Liberated;
        }

        CurrentProvinceId = provinceId;

        if (Provinces.TryGetValue(provinceId, out var province))
        {
            province.State = ProvinceState.Current;
        }
    }

    /// <summary>
    /// Liberates a province after winning a battle.
    /// </summary>
    public void LiberateProvince(string provinceId)
    {
        if (Provinces.TryGetValue(provinceId, out var province))
        {
            province.State = ProvinceState.Liberated;
        }
    }

    /// <summary>
    /// Checks if a realm is fully liberated.
    /// </summary>
    public bool IsRealmLiberated(string realmId)
    {
        if (!Realms.TryGetValue(realmId, out var realm))
            return false;

        return realm.ProvinceIds.All(pid =>
            Provinces.TryGetValue(pid, out var p) &&
            (p.State == ProvinceState.Liberated || p.State == ProvinceState.Current));
    }

    /// <summary>
    /// Gets provinces adjacent to the current province.
    /// </summary>
    public List<Province> GetAdjacentProvinces()
    {
        var current = CurrentProvince;
        if (current == null) return new List<Province>();

        return current.AdjacentProvinceIds
            .Where(id => Provinces.ContainsKey(id))
            .Select(id => Provinces[id])
            .ToList();
    }

    /// <summary>
    /// Checks if player can travel to a province (must be adjacent).
    /// </summary>
    public bool CanTravelTo(string provinceId)
    {
        var current = CurrentProvince;
        if (current == null) return false;

        return current.IsAdjacentTo(provinceId);
    }

    /// <summary>
    /// Creates the default campaign map with all provinces and realms.
    /// </summary>
    public static CampaignMap CreateDefaultMap()
    {
        var map = new CampaignMap();

        // =============================================
        // REALM: Valorian Highlands (Starting Realm)
        // House Valorian - Knights and defenders
        // =============================================
        map.Realms["valorian"] = new Realm(
            id: "valorian",
            name: "Valorian Highlands",
            houseAffiliation: "House Valorian",
            color: new Color(70, 130, 200),
            provinceIds: new List<string> { "valorian_keep", "valorian_outpost", "valorian_village" },
            realmBuffDescription: "+1 Defense to all heroes"
        );

        map.Provinces["valorian_keep"] = new Province(
            id: "valorian_keep",
            name: "Valorian Keep",
            realmId: "valorian",
            mapPosition: new Vector2(0.2f, 0.5f),
            adjacentProvinceIds: new List<string> { "valorian_outpost" },
            difficulty: 0, // Starting position, no battle
            description: "The ancestral seat of House Valorian. Your journey begins here.",
            state: ProvinceState.Current
        );

        map.Provinces["valorian_outpost"] = new Province(
            id: "valorian_outpost",
            name: "Northern Outpost",
            realmId: "valorian",
            mapPosition: new Vector2(0.3f, 0.3f),
            adjacentProvinceIds: new List<string> { "valorian_keep", "valorian_village", "sylvara_edge" },
            difficulty: 1,
            description: "A frontier outpost now occupied by orc raiders."
        );

        map.Provinces["valorian_village"] = new Province(
            id: "valorian_village",
            name: "Stonehaven",
            realmId: "valorian",
            mapPosition: new Vector2(0.35f, 0.55f),
            adjacentProvinceIds: new List<string> { "valorian_outpost", "ignara_border" },
            difficulty: 1,
            description: "A mining village under siege by goblin forces."
        );

        // =============================================
        // REALM: Sylvaran Forests
        // House Sylvara - Archers and rangers
        // =============================================
        map.Realms["sylvara"] = new Realm(
            id: "sylvara",
            name: "Sylvaran Forests",
            houseAffiliation: "House Sylvara",
            color: new Color(100, 180, 100),
            provinceIds: new List<string> { "sylvara_edge", "sylvara_heart", "sylvara_shrine" },
            realmBuffDescription: "+1 Attack Range to all heroes"
        );

        map.Provinces["sylvara_edge"] = new Province(
            id: "sylvara_edge",
            name: "Forest's Edge",
            realmId: "sylvara",
            mapPosition: new Vector2(0.45f, 0.2f),
            adjacentProvinceIds: new List<string> { "valorian_outpost", "sylvara_heart" },
            difficulty: 2,
            description: "The border of the great forest, now corrupted by dark magic."
        );

        map.Provinces["sylvara_heart"] = new Province(
            id: "sylvara_heart",
            name: "Heartwood Grove",
            realmId: "sylvara",
            mapPosition: new Vector2(0.55f, 0.15f),
            adjacentProvinceIds: new List<string> { "sylvara_edge", "sylvara_shrine", "umbra_pass" },
            difficulty: 2,
            description: "The ancient heart of the forest, overrun by corrupted beasts."
        );

        map.Provinces["sylvara_shrine"] = new Province(
            id: "sylvara_shrine",
            name: "Moon Shrine",
            realmId: "sylvara",
            mapPosition: new Vector2(0.7f, 0.1f),
            adjacentProvinceIds: new List<string> { "sylvara_heart" },
            difficulty: 3,
            description: "A sacred shrine to the moon goddess, defiled by the enemy."
        );

        // =============================================
        // REALM: Ignaran Wastes
        // House Ignara - Mages and fire wielders
        // =============================================
        map.Realms["ignara"] = new Realm(
            id: "ignara",
            name: "Ignaran Wastes",
            houseAffiliation: "House Ignara",
            color: new Color(180, 100, 180),
            provinceIds: new List<string> { "ignara_border", "ignara_ruins", "ignara_tower" },
            realmBuffDescription: "+1 Ability damage to all heroes"
        );

        map.Provinces["ignara_border"] = new Province(
            id: "ignara_border",
            name: "Ashland Border",
            realmId: "ignara",
            mapPosition: new Vector2(0.45f, 0.65f),
            adjacentProvinceIds: new List<string> { "valorian_village", "ignara_ruins" },
            difficulty: 2,
            description: "The scorched borderlands of the volcanic realm."
        );

        map.Provinces["ignara_ruins"] = new Province(
            id: "ignara_ruins",
            name: "Flamespire Ruins",
            realmId: "ignara",
            mapPosition: new Vector2(0.55f, 0.75f),
            adjacentProvinceIds: new List<string> { "ignara_border", "ignara_tower", "ferrum_gate" },
            difficulty: 2,
            description: "Ruins of an ancient academy, now home to fire elementals."
        );

        map.Provinces["ignara_tower"] = new Province(
            id: "ignara_tower",
            name: "Obsidian Tower",
            realmId: "ignara",
            mapPosition: new Vector2(0.65f, 0.85f),
            adjacentProvinceIds: new List<string> { "ignara_ruins" },
            difficulty: 3,
            description: "The legendary tower of arcane power, occupied by a dark sorcerer."
        );

        // =============================================
        // REALM: Umbral Depths
        // House Umbra - Rogues and shadow walkers
        // =============================================
        map.Realms["umbra"] = new Realm(
            id: "umbra",
            name: "Umbral Depths",
            houseAffiliation: "House Umbra",
            color: new Color(100, 80, 120),
            provinceIds: new List<string> { "umbra_pass", "umbra_caverns" },
            realmBuffDescription: "+1 Movement to all heroes"
        );

        map.Provinces["umbra_pass"] = new Province(
            id: "umbra_pass",
            name: "Shadow Pass",
            realmId: "umbra",
            mapPosition: new Vector2(0.7f, 0.35f),
            adjacentProvinceIds: new List<string> { "sylvara_heart", "umbra_caverns", "ferrum_gate" },
            difficulty: 3,
            description: "A treacherous mountain pass shrouded in eternal twilight."
        );

        map.Provinces["umbra_caverns"] = new Province(
            id: "umbra_caverns",
            name: "Void Caverns",
            realmId: "umbra",
            mapPosition: new Vector2(0.8f, 0.45f),
            adjacentProvinceIds: new List<string> { "umbra_pass", "crown_approach" },
            difficulty: 4,
            description: "Caverns where light itself is devoured by the darkness."
        );

        // =============================================
        // REALM: Ferrum Stronghold
        // House Ferrum - Berserkers and warriors
        // =============================================
        map.Realms["ferrum"] = new Realm(
            id: "ferrum",
            name: "Ferrum Stronghold",
            houseAffiliation: "House Ferrum",
            color: new Color(200, 80, 60),
            provinceIds: new List<string> { "ferrum_gate", "ferrum_forge" },
            realmBuffDescription: "+2 HP to all heroes"
        );

        map.Provinces["ferrum_gate"] = new Province(
            id: "ferrum_gate",
            name: "Iron Gate",
            realmId: "ferrum",
            mapPosition: new Vector2(0.7f, 0.55f),
            adjacentProvinceIds: new List<string> { "umbra_pass", "ignara_ruins", "ferrum_forge" },
            difficulty: 3,
            description: "The fortified entrance to the mountain stronghold."
        );

        map.Provinces["ferrum_forge"] = new Province(
            id: "ferrum_forge",
            name: "Great Forge",
            realmId: "ferrum",
            mapPosition: new Vector2(0.8f, 0.65f),
            adjacentProvinceIds: new List<string> { "ferrum_gate", "crown_approach" },
            difficulty: 4,
            description: "The legendary forge where the greatest weapons were once made."
        );

        // =============================================
        // FINAL REALM: Crown's Domain
        // The Shattered Crown - Final boss area
        // =============================================
        map.Realms["crown"] = new Realm(
            id: "crown",
            name: "Crown's Domain",
            houseAffiliation: "The Shattered Crown",
            color: new Color(220, 180, 100),
            provinceIds: new List<string> { "crown_approach", "crown_throne" },
            realmBuffDescription: "Victory!"
        );

        map.Provinces["crown_approach"] = new Province(
            id: "crown_approach",
            name: "Shattered Path",
            realmId: "crown",
            mapPosition: new Vector2(0.88f, 0.5f),
            adjacentProvinceIds: new List<string> { "umbra_caverns", "ferrum_forge", "crown_throne" },
            difficulty: 4,
            description: "The final approach to the throne, guarded by elite forces."
        );

        map.Provinces["crown_throne"] = new Province(
            id: "crown_throne",
            name: "Throne of Crowns",
            realmId: "crown",
            mapPosition: new Vector2(0.92f, 0.5f),
            adjacentProvinceIds: new List<string> { "crown_approach" },
            difficulty: 5,
            description: "The seat of power where the usurper awaits. End this."
        );

        // Set starting position
        map.CurrentProvinceId = "valorian_keep";

        return map;
    }
}
