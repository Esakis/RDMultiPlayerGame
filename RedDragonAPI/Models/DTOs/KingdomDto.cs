namespace RedDragonAPI.Models.DTOs;

public class KingdomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public bool IsMagicRace { get; set; }

    // Zasoby (Red Dragon faithful)
    public int Land { get; set; }
    public long Gold { get; set; }
    public long Food { get; set; }
    public long Stone { get; set; }
    public long Budulec { get; set; }
    public long BudulecStored { get; set; }
    public long Weapons { get; set; }
    public long Mana { get; set; }
    public long Bodies { get; set; }
    public string MetamagicMode { get; set; } = "None";
    public bool EntWrathActive { get; set; }
    public int TotemPlunder { get; set; }
    public int TotemDragonSlay { get; set; }
    public int TotemDestruction { get; set; }
    public string AppliedScienceSchool { get; set; } = "None";

    // Ludność
    public int Population { get; set; }
    public int Popularity { get; set; }
    public int Wages { get; set; }
    public decimal Education { get; set; }

    // Tury
    public int TurnsAvailable { get; set; }
    public int TurnsCapacity { get; set; }
    public int TurnsPerDay { get; set; }
    public int MaxTurns { get; set; }
    public int TurnNumber { get; set; }

    // Wiek
    public int Age { get; set; }

    // Budynek specjalny w budowie
    public string? CurrentSpecialBuilding { get; set; }
    public int SpecialBuildingProgress { get; set; }
    public int SpecialBuildingCost { get; set; }

    // Nauka (badanie) w toku
    public string? CurrentResearch { get; set; }
    public string? CurrentResearchTech { get; set; }
    public long ResearchProgress { get; set; }
    public long ResearchCost { get; set; }
    public long SciencePoints { get; set; }

    // Koalicja
    public int? CoalitionId { get; set; }
    public string? CoalitionName { get; set; }
    public string? CoalitionRole { get; set; }

    // Era
    public int EraId { get; set; }
    public string? EraName { get; set; }

    // Ochrona (nowicjusz) i mrożenie
    public bool IsProtected { get; set; }
    public int ProtectionDaysLeft { get; set; }
    public bool IsFrozen { get; set; }

    // Budynki
    public List<BuildingDto> Buildings { get; set; } = new();

    // Armia
    public List<MilitaryUnitDto> MilitaryUnits { get; set; } = new();

    // Zawody
    public List<ProfessionDto> Professions { get; set; } = new();

    // Wydarzenia / status (wyświetlane na Stolicy)
    public List<ActiveSpellDto> ActiveSpells { get; set; } = new();
    public int PendingGeneralCount { get; set; }
    public List<KingdomEventDto> RecentEvents { get; set; } = new();
}

public class ActiveSpellDto
{
    public string SpellType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Power { get; set; }
    public bool IsPositive { get; set; }
}

public class KingdomEventDto
{
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class KingdomSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Land { get; set; }
    public int Population { get; set; }
    public long Gold { get; set; }
    public int Military { get; set; }
    // Siła bojowa (wzory Dracopedii/manuala — BattleCalculator)
    public long AttackPower { get; set; }
    public long DefensePower { get; set; }
    // Siła magiczna = zgromadzona mana
    public long Magic { get; set; }
    // Siła złodziejska = liczba złodziei skorygowana o modyfikator rasy
    public long ThiefPower { get; set; }
    // Zabudowa
    public int BuildingCount { get; set; }
    public int UsedLand { get; set; }
    public int FreeLand { get; set; }
    public decimal BuiltPercent { get; set; }
    public int? CoalitionId { get; set; }
    public string? CoalitionTag { get; set; }
    public string? CoalitionRole { get; set; }
    public bool IsProtected { get; set; }
    public bool IsFrozen { get; set; }
}

public class SetMetamagicDto
{
    public string Mode { get; set; } = "None";
}

public class ChargeTotemDto
{
    public string Totem { get; set; } = string.Empty;
}

public class AppliedScienceDto
{
    public string School { get; set; } = "None";
}

public class ChangeRaceDto
{
    public string Race { get; set; } = string.Empty;
}

public class AssignWorkersDto
{
    public string ProfessionType { get; set; } = string.Empty;
    public int WorkerCount { get; set; }
}

public class BuyLandDto
{
    public int Amount { get; set; }
}
