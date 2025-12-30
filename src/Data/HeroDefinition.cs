using Microsoft.Xna.Framework;

namespace LegacyOfTheShatteredCrown.Data;

/// <summary>
/// Template for creating hero units. Defines base stats, abilities, and traits.
/// Used to instantiate actual Unit objects for battle.
/// </summary>
public class HeroDefinition
{
    /// <summary>
    /// Unique identifier for this hero type.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the hero.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Hero's background story or description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The Ancient House this hero belongs to.
    /// </summary>
    public string HouseAffiliation { get; set; }

    /// <summary>
    /// Base color for rendering this hero.
    /// </summary>
    public Color Color { get; set; }

    // ============================================
    // BASE STATS
    // ============================================

    public int BaseMaxHP { get; set; }
    public int BaseMoveRange { get; set; }
    public int BaseAttackRange { get; set; }
    public int BaseAttackPower { get; set; }
    public int BaseDefense { get; set; }
    public int BasePushResistance { get; set; }

    /// <summary>
    /// The passive trait this hero has.
    /// </summary>
    public PassiveTrait PassiveTrait { get; set; }

    /// <summary>
    /// Factory functions to create this hero's abilities.
    /// </summary>
    public List<Func<Ability>> AbilityFactories { get; set; } = new();

    public HeroDefinition(
        string id,
        string name,
        string description,
        string houseAffiliation,
        Color color,
        int baseMaxHP,
        int baseMoveRange,
        int baseAttackRange,
        int baseAttackPower,
        int baseDefense,
        int basePushResistance,
        PassiveTrait passiveTrait,
        List<Func<Ability>>? abilityFactories = null)
    {
        Id = id;
        Name = name;
        Description = description;
        HouseAffiliation = houseAffiliation;
        Color = color;
        BaseMaxHP = baseMaxHP;
        BaseMoveRange = baseMoveRange;
        BaseAttackRange = baseAttackRange;
        BaseAttackPower = baseAttackPower;
        BaseDefense = baseDefense;
        BasePushResistance = basePushResistance;
        PassiveTrait = passiveTrait;
        AbilityFactories = abilityFactories ?? new();
    }

    /// <summary>
    /// Creates a Unit instance from this hero definition.
    /// </summary>
    /// <param name="gridPosition">Starting position on the grid.</param>
    /// <returns>A new Unit with this hero's stats and abilities.</returns>
    public Unit CreateUnit(Point gridPosition)
    {
        var unit = new Unit(
            name: Name,
            gridPosition: gridPosition,
            moveRange: BaseMoveRange,
            color: Color,
            maxHP: BaseMaxHP,
            attackRange: BaseAttackRange,
            attackPower: BaseAttackPower,
            defense: BaseDefense,
            isPlayerUnit: true,
            pushResistance: BasePushResistance,
            houseAffiliation: HouseAffiliation,
            passiveTrait: PassiveTrait
        );

        // Create fresh ability instances for this unit
        foreach (var factory in AbilityFactories)
        {
            unit.Abilities.Add(factory());
        }

        return unit;
    }

    // ============================================
    // PREDEFINED HERO DEFINITIONS
    // ============================================

    /// <summary>
    /// Sir Aldric - Knight of House Valorian. Tank with push abilities.
    /// </summary>
    public static HeroDefinition SirAldric()
    {
        return new HeroDefinition(
            id: "hero_aldric",
            name: "Sir Aldric",
            description: "A stalwart knight of House Valorian, known for his unbreakable defense.",
            houseAffiliation: "House Valorian",
            color: new Color(70, 130, 200),
            baseMaxHP: 14,
            baseMoveRange: 3,
            baseAttackRange: 1,
            baseAttackPower: 3,
            baseDefense: 2,
            basePushResistance: 1,
            passiveTrait: PassiveTrait.HoldGround(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.ShieldBash,
                Ability.Charge
            }
        );
    }

    /// <summary>
    /// Lyra Swiftbow - Archer of House Sylvara. Ranged damage dealer.
    /// </summary>
    public static HeroDefinition LyraSwiftbow()
    {
        return new HeroDefinition(
            id: "hero_lyra",
            name: "Lyra Swiftbow",
            description: "An elite archer from the forests of Sylvara, deadly at range.",
            houseAffiliation: "House Sylvara",
            color: new Color(100, 180, 100),
            baseMaxHP: 8,
            baseMoveRange: 2,
            baseAttackRange: 4,
            baseAttackPower: 4,
            baseDefense: 0,
            basePushResistance: 0,
            passiveTrait: PassiveTrait.VolleyReady(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.Volley
            }
        );
    }

    /// <summary>
    /// Mira Flamecaller - Mage of House Ignara. High damage caster.
    /// </summary>
    public static HeroDefinition MiraFlamecaller()
    {
        return new HeroDefinition(
            id: "hero_mira",
            name: "Mira Flamecaller",
            description: "A powerful mage channeling the ancient flames of House Ignara.",
            houseAffiliation: "House Ignara",
            color: new Color(180, 100, 180),
            baseMaxHP: 7,
            baseMoveRange: 2,
            baseAttackRange: 3,
            baseAttackPower: 3,
            baseDefense: 0,
            basePushResistance: 0,
            passiveTrait: PassiveTrait.ArcaneConduit(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.Fireball,
                Ability.Grapple
            }
        );
    }

    /// <summary>
    /// Brother Theron - Priest of House Solara. Healer and support.
    /// </summary>
    public static HeroDefinition BrotherTheron()
    {
        return new HeroDefinition(
            id: "hero_theron",
            name: "Brother Theron",
            description: "A devoted healer from the sacred order of House Solara.",
            houseAffiliation: "House Solara",
            color: new Color(255, 220, 150),
            baseMaxHP: 9,
            baseMoveRange: 2,
            baseAttackRange: 1,
            baseAttackPower: 2,
            baseDefense: 1,
            basePushResistance: 0,
            passiveTrait: PassiveTrait.DivineShield(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.HealingLight
            }
        );
    }

    /// <summary>
    /// Kira Shadowblade - Rogue of House Umbra. Mobile flanker.
    /// </summary>
    public static HeroDefinition KiraShadowblade()
    {
        return new HeroDefinition(
            id: "hero_kira",
            name: "Kira Shadowblade",
            description: "A cunning assassin from the shadowy House Umbra.",
            houseAffiliation: "House Umbra",
            color: new Color(100, 80, 120),
            baseMaxHP: 8,
            baseMoveRange: 4,
            baseAttackRange: 1,
            baseAttackPower: 5,
            baseDefense: 0,
            basePushResistance: 0,
            passiveTrait: PassiveTrait.Flanker(),
            abilityFactories: new List<Func<Ability>>
            {
                // Could add a dash/stealth ability later
            }
        );
    }

    /// <summary>
    /// Gorath the Unyielding - Berserker of House Ferrum. Damage tank.
    /// </summary>
    public static HeroDefinition GorathUnyielding()
    {
        return new HeroDefinition(
            id: "hero_gorath",
            name: "Gorath the Unyielding",
            description: "A fearsome berserker who grows stronger as he takes damage.",
            houseAffiliation: "House Ferrum",
            color: new Color(200, 80, 60),
            baseMaxHP: 12,
            baseMoveRange: 2,
            baseAttackRange: 1,
            baseAttackPower: 4,
            baseDefense: 1,
            basePushResistance: 1,
            passiveTrait: PassiveTrait.BerserkerRage(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.Charge
            }
        );
    }

    /// <summary>
    /// Captain Roderick - Spearman of House Astridon. Defensive line holder.
    /// </summary>
    public static HeroDefinition CaptainRoderick()
    {
        return new HeroDefinition(
            id: "hero_roderick",
            name: "Captain Roderick",
            description: "A veteran spearman who holds the line against any foe.",
            houseAffiliation: "House Astridon",
            color: new Color(180, 160, 100),
            baseMaxHP: 11,
            baseMoveRange: 2,
            baseAttackRange: 2,
            baseAttackPower: 3,
            baseDefense: 1,
            basePushResistance: 2,
            passiveTrait: PassiveTrait.Phalanx(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.SpearThrust,
                Ability.Brace
            }
        );
    }

    /// <summary>
    /// Lady Elara - Cavalry of House Equitarn. High mobility striker.
    /// </summary>
    public static HeroDefinition LadyElara()
    {
        return new HeroDefinition(
            id: "hero_elara",
            name: "Lady Elara",
            description: "A swift cavalry commander who strikes like lightning.",
            houseAffiliation: "House Equitarn",
            color: new Color(220, 180, 80),
            baseMaxHP: 9,
            baseMoveRange: 5,
            baseAttackRange: 1,
            baseAttackPower: 4,
            baseDefense: 0,
            basePushResistance: 0,
            passiveTrait: PassiveTrait.Momentum(),
            abilityFactories: new List<Func<Ability>>
            {
                Ability.Trample,
                Ability.LanceStrike
            }
        );
    }

    /// <summary>
    /// Returns a list of all available hero definitions.
    /// </summary>
    public static List<HeroDefinition> GetAllHeroes()
    {
        return new List<HeroDefinition>
        {
            SirAldric(),
            LyraSwiftbow(),
            MiraFlamecaller(),
            BrotherTheron(),
            KiraShadowblade(),
            GorathUnyielding(),
            CaptainRoderick(),
            LadyElara()
        };
    }
}
