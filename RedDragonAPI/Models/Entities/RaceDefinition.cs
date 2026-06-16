using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Definicja rasy wg oryginalnego Red Dragon (10 ras).
/// Statystyki z oficjalnej strony reddragon.cz oraz rebalansu „31. wieku"
/// (szczegóły i źródła: docs/MECHANIKA.md).
/// </summary>
[Table("RaceDefinitions")]
public class RaceDefinition
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;          // PL, np. "Człowiek"

    [MaxLength(50)]
    public string NameCz { get; set; } = string.Empty;        // oryginał CZ, np. "Člověk"

    public string? Description { get; set; }                  // oficjalny opis PL

    // Charakterystyka 0-100 z oficjalnej strony (data-characteristics)
    public int EaseRating { get; set; }
    public int MagicRating { get; set; }
    public int ThievesRating { get; set; }
    public int DefenseRating { get; set; }
    public int EconomyRating { get; set; }
    public int AttackRating { get; set; }

    // Magia i czas
    public int MagicBooks { get; set; }                       // 0-5 ksiąg magii
    public int TurnsPerDay { get; set; } = 15;                // Goblin 17, Ent 13
    public int GeneralsLimit { get; set; } = 6;               // Człowiek 8
    public int LimitedSpellsPerRecalc { get; set; } = 2;      // Krasnolud 1, Ent 3, Olbrzym 4

    // Populacja
    [Column(TypeName = "decimal(5,2)")]
    public decimal HouseCapacityBase { get; set; } = 3;       // pojemność domu
    [Column(TypeName = "decimal(5,2)")]
    public decimal PopPerAcreBase { get; set; } = 3;          // zaludnienie akra
    [Column(TypeName = "decimal(5,2)")]
    public decimal WaterworksHouseBonus { get; set; } = 0.5m; // Wodociągi (Vodárna)
    [Column(TypeName = "decimal(5,2)")]
    public decimal BurrowsHouseBonus { get; set; } = 1m;      // System nor (Norový systém)
    [Column(TypeName = "decimal(5,2)")]
    public decimal SewersHouseBonus { get; set; } = 1.5m;     // Kanalizacja
    [Column(TypeName = "decimal(5,2)")]
    public decimal AqueductAcreBonus { get; set; } = 0.5m;    // Wodotok
    [Column(TypeName = "decimal(5,2)")]
    public decimal FoodPerPop { get; set; } = 1;              // Olbrzym 2
    [Column(TypeName = "decimal(5,2)")]
    public decimal PopGrowthModifier { get; set; } = 0;       // Goblin +0.25

    // Bonusy profesji (ułamek, np. +0.30 = +30%)
    [Column(TypeName = "decimal(5,2)")] public decimal BonusFarmers { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusStonemasons { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusMasons { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusMerchants { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusAlchemists { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusArmorers { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusDruids { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusMages { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal BonusScientists { get; set; }

    // Wojsko (hoplita zawsze 1/1)
    public int E1Attack { get; set; }
    public int E1Defense { get; set; }
    public int E2Attack { get; set; }
    public int E2Defense { get; set; }
    public int MachineAttack { get; set; } = 5;

    // Modyfikatory
    [Column(TypeName = "decimal(5,2)")]
    public decimal ThiefPowerModifier { get; set; }           // np. Hobbit +0.25, Nekromant -0.50
    [Column(TypeName = "decimal(5,2)")]
    public decimal ThiefCostModifier { get; set; }            // np. Hobbit -0.25
    [Column(TypeName = "decimal(5,2)")]
    public decimal MilitaryLossModifier { get; set; }         // Krasnolud -0.25, Ent -0.50
    [Column(TypeName = "decimal(5,2)")]
    public decimal ResearchModifier { get; set; }             // Człowiek +0.10, Goblin -0.20, Dżin +0.20

    public string? SpecialTraits { get; set; }                // opis cech specjalnych (PL)
}
