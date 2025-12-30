namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Types of targeting for abilities.
/// </summary>
public enum TargetingType
{
    /// <summary>No target needed, affects self.</summary>
    Self,
    /// <summary>Targets a single enemy unit.</summary>
    SingleEnemy,
    /// <summary>Targets a single ally unit.</summary>
    SingleAlly,
    /// <summary>Targets any single unit.</summary>
    SingleUnit,
    /// <summary>Targets a specific tile (empty or occupied).</summary>
    Tile,
    /// <summary>Targets in a line from the caster.</summary>
    Line,
    /// <summary>Targets all units in an area.</summary>
    Area
}

/// <summary>
/// Types of effects an ability can have.
/// </summary>
public enum AbilityEffect
{
    /// <summary>Deals damage to targets.</summary>
    Damage,
    /// <summary>Heals targets.</summary>
    Heal,
    /// <summary>Pushes targets away from caster.</summary>
    Push,
    /// <summary>Pulls targets toward caster.</summary>
    Pull,
    /// <summary>Moves the caster to target location.</summary>
    Dash,
    /// <summary>Applies a buff to targets.</summary>
    Buff,
    /// <summary>Applies a debuff to targets.</summary>
    Debuff,
    /// <summary>Creates terrain or hazard.</summary>
    CreateTerrain
}

/// <summary>
/// Represents an ability that a hero can use in combat.
/// Abilities have cooldowns and various effects.
/// </summary>
public class Ability
{
    /// <summary>
    /// Display name of the ability.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of what the ability does.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Range at which this ability can be used (Manhattan distance).
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Number of turns before the ability can be used again after use.
    /// </summary>
    public int Cooldown { get; set; }

    /// <summary>
    /// Current cooldown remaining. 0 means ability is ready.
    /// </summary>
    public int CurrentCooldown { get; set; }

    /// <summary>
    /// How the ability selects its target.
    /// </summary>
    public TargetingType Targeting { get; set; }

    /// <summary>
    /// Primary effect of the ability.
    /// </summary>
    public AbilityEffect Effect { get; set; }

    /// <summary>
    /// Power/magnitude of the effect (damage amount, heal amount, push distance, etc.).
    /// </summary>
    public int Power { get; set; }

    /// <summary>
    /// Area of effect radius for Area targeting type. 0 for single target.
    /// </summary>
    public int AreaRadius { get; set; }

    /// <summary>
    /// Whether the ability is ready to use.
    /// </summary>
    public bool IsReady => CurrentCooldown == 0;

    public Ability(
        string name,
        string description,
        int range,
        int cooldown,
        TargetingType targeting,
        AbilityEffect effect,
        int power,
        int areaRadius = 0)
    {
        Name = name;
        Description = description;
        Range = range;
        Cooldown = cooldown;
        CurrentCooldown = 0;
        Targeting = targeting;
        Effect = effect;
        Power = power;
        AreaRadius = areaRadius;
    }

    /// <summary>
    /// Uses the ability, putting it on cooldown.
    /// </summary>
    public void Use()
    {
        CurrentCooldown = Cooldown;
    }

    /// <summary>
    /// Reduces cooldown by 1 turn. Called at the start of each turn.
    /// </summary>
    public void TickCooldown()
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown--;
        }
    }

    /// <summary>
    /// Resets cooldown to 0, making the ability ready.
    /// </summary>
    public void ResetCooldown()
    {
        CurrentCooldown = 0;
    }

    // ============================================
    // FACTORY METHODS FOR COMMON ABILITIES
    // ============================================

    /// <summary>
    /// Creates a Shield Bash ability - melee push attack.
    /// </summary>
    public static Ability ShieldBash()
    {
        return new Ability(
            name: "Shield Bash",
            description: "Strike an adjacent enemy, pushing them back 1 tile.",
            range: 1,
            cooldown: 2,
            targeting: TargetingType.SingleEnemy,
            effect: AbilityEffect.Push,
            power: 1
        );
    }

    /// <summary>
    /// Creates a Charge ability - dash toward enemy and attack.
    /// </summary>
    public static Ability Charge()
    {
        return new Ability(
            name: "Charge",
            description: "Rush toward an enemy up to 3 tiles away, dealing bonus damage.",
            range: 3,
            cooldown: 3,
            targeting: TargetingType.SingleEnemy,
            effect: AbilityEffect.Damage,
            power: 5
        );
    }

    /// <summary>
    /// Creates a Volley ability - ranged area attack.
    /// </summary>
    public static Ability Volley()
    {
        return new Ability(
            name: "Volley",
            description: "Rain arrows on an area, damaging all enemies within.",
            range: 4,
            cooldown: 3,
            targeting: TargetingType.Area,
            effect: AbilityEffect.Damage,
            power: 2,
            areaRadius: 1
        );
    }

    /// <summary>
    /// Creates a Healing Light ability - heal an ally.
    /// </summary>
    public static Ability HealingLight()
    {
        return new Ability(
            name: "Healing Light",
            description: "Restore HP to an ally within range.",
            range: 3,
            cooldown: 2,
            targeting: TargetingType.SingleAlly,
            effect: AbilityEffect.Heal,
            power: 4
        );
    }

    /// <summary>
    /// Creates a Fireball ability - ranged damage.
    /// </summary>
    public static Ability Fireball()
    {
        return new Ability(
            name: "Fireball",
            description: "Hurl a ball of fire at an enemy, dealing heavy damage.",
            range: 4,
            cooldown: 2,
            targeting: TargetingType.SingleEnemy,
            effect: AbilityEffect.Damage,
            power: 6
        );
    }

    /// <summary>
    /// Creates a Grapple ability - pull enemy toward you.
    /// </summary>
    public static Ability Grapple()
    {
        return new Ability(
            name: "Grapple",
            description: "Pull an enemy 2 tiles toward you.",
            range: 3,
            cooldown: 2,
            targeting: TargetingType.SingleEnemy,
            effect: AbilityEffect.Pull,
            power: 2
        );
    }

    /// <summary>
    /// Creates a Brace ability - defensive stance with counter-push.
    /// </summary>
    public static Ability Brace()
    {
        return new Ability(
            name: "Brace",
            description: "Brace for impact. Next attack against you is reduced and pushes attacker back.",
            range: 0,
            cooldown: 2,
            targeting: TargetingType.Self,
            effect: AbilityEffect.Buff,
            power: 2
        );
    }

    /// <summary>
    /// Creates a Spear Thrust ability - attack with knockback.
    /// </summary>
    public static Ability SpearThrust()
    {
        return new Ability(
            name: "Spear Thrust",
            description: "A powerful thrust that pushes the enemy back 2 tiles.",
            range: 1,
            cooldown: 2,
            targeting: TargetingType.SingleEnemy,
            effect: AbilityEffect.Push,
            power: 2
        );
    }

    /// <summary>
    /// Creates a Trample ability - charge through enemies.
    /// </summary>
    public static Ability Trample()
    {
        return new Ability(
            name: "Trample",
            description: "Charge in a line, damaging and pushing all enemies in path.",
            range: 4,
            cooldown: 3,
            targeting: TargetingType.Line,
            effect: AbilityEffect.Damage,
            power: 3
        );
    }

    /// <summary>
    /// Creates a Lance Strike ability - powerful single-target attack.
    /// </summary>
    public static Ability LanceStrike()
    {
        return new Ability(
            name: "Lance Strike",
            description: "A devastating lance attack dealing heavy damage.",
            range: 2,
            cooldown: 2,
            targeting: TargetingType.SingleEnemy,
            effect: AbilityEffect.Damage,
            power: 6
        );
    }
}
