namespace RedDragonAPI.Models.DTOs;

public class BuildingDto
{
    public int Id { get; set; }
    public string BuildingType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int Level { get; set; }
    public bool IsUnderConstruction { get; set; }
    public DateTime? ConstructionCompletesAt { get; set; }
}

public class BuildingDefinitionDto
{
    public int Id { get; set; }
    public string BuildingType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int CostGold { get; set; }
    public int CostBudulec { get; set; }
    public int CostLand { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int BaseCost { get; set; }
    public int BuildTime { get; set; }

    public string? RequiredBuildingType { get; set; }
    public string? RequiredTechnology { get; set; }

    public bool IsSpecial { get; set; }
    public int BonusTurnsPerDay { get; set; }
    public decimal ProductionBonus { get; set; }
    public decimal DefenseBonus { get; set; }
    public int PopulationCapacity { get; set; }

    public bool CanBuild { get; set; }
    public string? CannotBuildReason { get; set; }
}

public class ConstructBuildingDto
{
    public string BuildingType { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
