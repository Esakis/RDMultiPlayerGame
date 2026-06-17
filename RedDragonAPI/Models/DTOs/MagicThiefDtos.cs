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
    public long ExperienceToNextLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPending { get; set; }
    public int SecondaryRerollsLeft { get; set; }
}

/// <summary>Stan paktów księstwa: lista współczłonków koalicji z aktualnym typem paktu
/// oraz informacja o limicie paktów obronnych.</summary>
public class PactStatusDto
{
    public bool InCoalition { get; set; }
    /// <summary>Maks. liczba paktów obronnych (baza 5 + Ambasada).</summary>
    public int Limit { get; set; }
    /// <summary>Liczba aktualnie zawartych paktów obronnych.</summary>
    public int UsedSlots { get; set; }
    public bool HasAmbasada { get; set; }
    public List<PactMemberDto> Members { get; set; } = new();
}

public class PactMemberDto
{
    public int KingdomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Land { get; set; }
    /// <summary>Handlowy (domyślny) | Magiczny | Wojskowy | Zlodziejski</summary>
    public string PactType { get; set; } = "Handlowy";
}

public class SetPactDto
{
    public int TargetKingdomId { get; set; }
    public string PactType { get; set; } = "Handlowy";
}
