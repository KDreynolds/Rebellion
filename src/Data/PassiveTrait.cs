namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Represents a passive trait that provides ongoing benefits to a hero.
/// Each hero has one passive trait that defines part of their playstyle.
/// </summary>
public class PassiveTrait
{
    /// <summary>
    /// Display name of the trait.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the trait's effect.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The type of passive effect this trait provides.
    /// </summary>
    public PassiveType Type { get; set; }

    /// <summary>
    /// Magnitude of the passive effect (context-dependent).
    /// </summary>
    public int Value { get; set; }

    public PassiveTrait(string name, string description, PassiveType type, int value = 0)
    {
        Name = name;
        Description = description;
        Type = type;
        Value = value;
    }

    // ============================================
    // PREDEFINED PASSIVE TRAITS
    // ============================================

    /// <summary>
    /// Hold Ground: Immune to push effects.
    /// </summary>
    public static PassiveTrait HoldGround()
    {
        return new PassiveTrait(
            name: "Hold Ground",
            description: "Cannot be pushed or pulled by enemy abilities.",
            type: PassiveType.PushImmunity,
            value: 0
        );
    }

    /// <summary>
    /// Momentum: Bonus damage when moving before attacking.
    /// </summary>
    public static PassiveTrait Momentum()
    {
        return new PassiveTrait(
            name: "Momentum",
            description: "Deal +2 damage when attacking after moving.",
            type: PassiveType.MomentumDamage,
            value: 2
        );
    }

    /// <summary>
    /// Phalanx: Reduced damage from frontal attacks.
    /// </summary>
    public static PassiveTrait Phalanx()
    {
        return new PassiveTrait(
            name: "Phalanx",
            description: "Take 2 less damage from attacks in front.",
            type: PassiveType.FrontalDefense,
            value: 2
        );
    }

    /// <summary>
    /// Arcane Conduit: Abilities deal bonus damage.
    /// </summary>
    public static PassiveTrait ArcaneConduit()
    {
        return new PassiveTrait(
            name: "Arcane Conduit",
            description: "Ability damage increased by 1.",
            type: PassiveType.AbilityDamageBonus,
            value: 1
        );
    }

    /// <summary>
    /// Berserker Rage: More damage when wounded.
    /// </summary>
    public static PassiveTrait BerserkerRage()
    {
        return new PassiveTrait(
            name: "Berserker Rage",
            description: "Deal +1 damage for each 25% HP missing.",
            type: PassiveType.WoundedDamage,
            value: 1
        );
    }

    /// <summary>
    /// Volley Ready: First ranged attack each combat has bonus range.
    /// </summary>
    public static PassiveTrait VolleyReady()
    {
        return new PassiveTrait(
            name: "Volley Ready",
            description: "First attack each battle has +2 range.",
            type: PassiveType.FirstAttackRangeBonus,
            value: 2
        );
    }

    /// <summary>
    /// Divine Shield: Heal for 1 HP at the start of each turn.
    /// </summary>
    public static PassiveTrait DivineShield()
    {
        return new PassiveTrait(
            name: "Divine Shield",
            description: "Regenerate 1 HP at the start of each turn.",
            type: PassiveType.Regeneration,
            value: 1
        );
    }

    /// <summary>
    /// Flanker: Bonus damage when attacking from the side or behind.
    /// </summary>
    public static PassiveTrait Flanker()
    {
        return new PassiveTrait(
            name: "Flanker",
            description: "Deal +2 damage when attacking from the side or rear.",
            type: PassiveType.FlankingDamage,
            value: 2
        );
    }
}

/// <summary>
/// Types of passive effects that traits can provide.
/// </summary>
public enum PassiveType
{
    /// <summary>Immune to push/pull effects.</summary>
    PushImmunity,
    /// <summary>Bonus damage after moving.</summary>
    MomentumDamage,
    /// <summary>Reduced frontal damage.</summary>
    FrontalDefense,
    /// <summary>Bonus damage on abilities.</summary>
    AbilityDamageBonus,
    /// <summary>Bonus damage when wounded.</summary>
    WoundedDamage,
    /// <summary>Bonus range on first attack.</summary>
    FirstAttackRangeBonus,
    /// <summary>HP regeneration per turn.</summary>
    Regeneration,
    /// <summary>Bonus damage when flanking.</summary>
    FlankingDamage,
    /// <summary>Bonus movement speed.</summary>
    MovementBonus,
    /// <summary>Reduced ability cooldowns.</summary>
    CooldownReduction
}
