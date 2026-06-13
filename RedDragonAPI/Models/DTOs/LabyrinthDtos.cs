namespace RedDragonAPI.Models.DTOs;

public class LabyrinthGeneralDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string PrimaryTrait { get; set; } = string.Empty;
}

public class LabyrinthExpeditionDto
{
    public int GeneralId { get; set; }
    public string GeneralName { get; set; } = string.Empty;
    public int GeneralLevel { get; set; }
    public int Depth { get; set; }
    public long PendingGold { get; set; }
    public long PendingFood { get; set; }
    public long PendingStone { get; set; }
    public long PendingWeapons { get; set; }
    public long PendingMana { get; set; }
    public int PendingDice { get; set; }
    public string? LastEvent { get; set; }
}

public class LabyrinthStatusDto
{
    public bool HasActiveExpedition { get; set; }
    public LabyrinthExpeditionDto? Expedition { get; set; }

    /// <summary>Generałowie gotowi do wejścia (w domu, zdrowi).</summary>
    public List<LabyrinthGeneralDto> AvailableGenerals { get; set; } = new();

    public long BankedDice { get; set; }
    public int TurnsAvailable { get; set; }

    /// <summary>Poziom badań o smokach (zwiększa łupy i kości w labiryncie).</summary>
    public int DragonLore { get; set; }

    /// <summary>Nagrody do kupienia za zebrane kości.</summary>
    public List<LabyrinthRewardDto> Rewards { get; set; } = new();
}

public class LabyrinthRewardDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DiceCost { get; set; }
    public bool CanAfford { get; set; }
}

public class EnterLabyrinthDto
{
    public int GeneralId { get; set; }
}

public class SpendDiceDto
{
    public string RewardType { get; set; } = string.Empty;
}
