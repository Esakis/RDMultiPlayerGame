namespace RedDragonAPI.Models.DTOs;

public class ResearchDto
{
    public int Id { get; set; }
    public string TechType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsInProgress { get; set; }
    public DateTime? CompletesAt { get; set; }
}

public class TechDefinitionDto
{
    public int Id { get; set; }
    public string TechType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long CostScience { get; set; }
    public long InvestedScience { get; set; }
    public bool IsCurrent { get; set; }
    public string? RequiredTech { get; set; }
    public string? RequiredBuilding { get; set; }
    public string? EffectType { get; set; }
    public decimal EffectValue { get; set; }
    public bool IsCompleted { get; set; }
    public bool CanResearch { get; set; }
    public string? CannotResearchReason { get; set; }
}

public class ResearchStatusDto
{
    public long SciencePoints { get; set; }
    public string? CurrentResearchTech { get; set; }
    public long SciencePerTurnCap { get; set; }
}

public class StartResearchDto
{
    public string TechType { get; set; } = string.Empty;
}
