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

    // Ludność
    public int Population { get; set; }
    public int Popularity { get; set; }
    public int Wages { get; set; }
    public decimal Education { get; set; }

    // Tury
    public int TurnsAvailable { get; set; }
    public int TurnsPerDay { get; set; }
    public int MaxTurns { get; set; }
    public int TurnNumber { get; set; }

    // Wiek
    public int Age { get; set; }

    // Budynek specjalny w budowie
    public string? CurrentSpecialBuilding { get; set; }
    public int SpecialBuildingProgress { get; set; }
    public int SpecialBuildingCost { get; set; }

    // Koalicja
    public int? CoalitionId { get; set; }
    public string? CoalitionName { get; set; }
    public string? CoalitionRole { get; set; }

    // Era
    public int EraId { get; set; }
    public string? EraName { get; set; }

    // Ochrona
    public bool IsProtected { get; set; }

    // Budynki
    public List<BuildingDto> Buildings { get; set; } = new();

    // Armia
    public List<MilitaryUnitDto> MilitaryUnits { get; set; } = new();

    // Zawody
    public List<ProfessionDto> Professions { get; set; } = new();
}

public class KingdomSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Land { get; set; }
    public int Population { get; set; }
    public string? CoalitionTag { get; set; }
    public string? CoalitionRole { get; set; }
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
