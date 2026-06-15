namespace RedDragonAPI.Models.DTOs;

public class SpellListItemDto
{
    public string SpellType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BaseCost { get; set; }
    public long CurrentCost { get; set; }
    public bool IsLimited { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public bool CanCast { get; set; }
    public string? CannotCastReason { get; set; }
}

public class CastSpellDto
{
    public string SpellType { get; set; } = string.Empty;
    public int? TargetKingdomId { get; set; }
}

public class ThiefActionListItemDto
{
    public string ActionType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ThievesRequired { get; set; }
}

public class SendThievesDto
{
    public string ActionType { get; set; } = string.Empty;
    public int TargetKingdomId { get; set; }
    public int Thieves { get; set; }
}

public class SpellAttackData
{
    public string SpellType { get; set; } = string.Empty;
    public long Power { get; set; }
}

public class ThiefAttackData
{
    public string ActionType { get; set; } = string.Empty;
    public int Thieves { get; set; }
}

public class GeneralDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryTrait { get; set; } = string.Empty;
    public string SecondaryTrait { get; set; } = string.Empty;
    public long Experience { get; set; }
    public int Level { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPending { get; set; }
}

public class PactDto
{
    public int Id { get; set; }
    public string PactType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PartnerKingdomId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public bool IsProposer { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProposePactDto
{
    public string PactType { get; set; } = string.Empty;
    public int TargetKingdomId { get; set; }
}
