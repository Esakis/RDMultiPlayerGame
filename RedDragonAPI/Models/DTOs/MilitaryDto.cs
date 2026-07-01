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
    public int CostWeapons { get; set; }
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

/// <summary>Zbiorcza rekrutacja/zwolnienie wielu typów jednostek naraz (jeden przycisk).</summary>
public class UnitBatchDto
{
    public Dictionary<string, int> Units { get; set; } = new();
}

/// <summary>Stan i parametry automatycznego szkolenia wojska.</summary>
public class TrainingInfoDto
{
    public bool TrainSoldiers { get; set; }
    public bool TrainElite { get; set; }
    // Procent jednostek awansujących w każdej turze (zależny od nauki Trening).
    public decimal SoldierPromotePct { get; set; }
    public decimal ElitePromotePct { get; set; }
    // Czy dany stopień szkolenia jest dostępny (wymagany budynek istnieje).
    public bool CanTrainSoldiers { get; set; }
    public bool CanTrainElite { get; set; }
}

/// <summary>Ustawienie przełączników szkolenia.</summary>
public class SetTrainingDto
{
    public bool TrainSoldiers { get; set; }
    public bool TrainElite { get; set; }
}

public class AttackDto
{
    public int TargetKingdomId { get; set; }
    /// <summary>Generał prowadzący atak — wymagany, atak bez generała nie wyrusza.</summary>
    public int GeneralId { get; set; }
    public Dictionary<string, int> Units { get; set; } = new();
}

/// <summary>Zaplanowany (zakolejkowany) atak oczekujący na przeliczenie o 5:00.</summary>
public class PlannedAttackDto
{
    public int Id { get; set; }
    public int TargetKingdomId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public int GeneralId { get; set; }
    public string GeneralName { get; set; } = string.Empty;
    public Dictionary<string, int> Units { get; set; } = new();
    public DateTime ScheduledFor { get; set; }
    public DateTime CreatedAt { get; set; }
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
