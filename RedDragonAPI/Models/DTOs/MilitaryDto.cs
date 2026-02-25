namespace RedDragonAPI.Models.DTOs;

public class MilitaryUnitDto
{
    public int Id { get; set; }
    public string UnitType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int InTraining { get; set; }
    public DateTime? TrainingCompletesAt { get; set; }
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public int Upkeep { get; set; }
}

public class UnitDefinitionDto
{
    public int Id { get; set; }
    public string UnitType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CostGold { get; set; }
    public int CostIron { get; set; }
    public int CostWood { get; set; }
    public int CostFood { get; set; }
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public int Upkeep { get; set; }
    public string RequiredBuilding { get; set; } = string.Empty;
    public string? RequiredTech { get; set; }
    public int TrainingTime { get; set; }
    public bool CanRecruit { get; set; }
    public string? CannotRecruitReason { get; set; }
}

public class RecruitUnitsDto
{
    public string UnitType { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class AttackDto
{
    public int TargetKingdomId { get; set; }
    public Dictionary<string, int> Units { get; set; } = new();
}

public class BattleReportDto
{
    public int Id { get; set; }
    public string AttackerName { get; set; } = string.Empty;
    public string DefenderName { get; set; } = string.Empty;
    public string BattleType { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? AttackerLosses { get; set; }
    public string? DefenderLosses { get; set; }
    public string? ResourcesStolen { get; set; }
    public int LandCaptured { get; set; }
    public DateTime OccurredAt { get; set; }
}
