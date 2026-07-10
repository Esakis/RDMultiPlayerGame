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

public class SetAutoCastDto
{
    /// <summary>Zaklęcie do auto-rzucania po przeliczeniu; null/puste = wyłącz.</summary>
    public string? SpellType { get; set; }
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
    /// <summary>Czy generał jest w domu i zdolny do akcji (może np. poprowadzić atak).</summary>
    public bool IsAvailable { get; set; }
}

/// <summary>Stan paktów księstwa: pakt handlowy (bez partnera) + lista współczłonków
/// koalicji z aktywnymi typami paktów obronnych i informacją o limicie.</summary>
public class PactStatusDto
{
    public bool InCoalition { get; set; }
    /// <summary>Maks. łączna liczba paktów obronnych (baza 5 + Ambasada).</summary>
    public int Limit { get; set; }
    /// <summary>Liczba aktualnie zawartych paktów obronnych.</summary>
    public int UsedSlots { get; set; }
    public bool HasAmbasada { get; set; }
    /// <summary>Pakt handlowy (kupiecki) — bez partnera, udział w wymianie koalicji.</summary>
    public bool TradePactEnabled { get; set; }
    /// <summary>Czy pakt handlowy działa jeszcze połowicznie (do najbliższego przeliczenia).</summary>
    public bool TradePactHalf { get; set; }
    public List<PactMemberDto> Members { get; set; } = new();
}

public class PactMemberDto
{
    public int KingdomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Land { get; set; }
    /// <summary>Czy członek uczestniczy w wymianie handlowej koalicji (jego ziemia
    /// liczy się Twoim kupcom, gdy sam też masz włączony handel).</summary>
    public bool TradePactEnabled { get; set; }
    /// <summary>Aktywne typy paktów OBRONNYCH z tym księstwem: Magiczny | Wojskowy | Zlodziejski.</summary>
    public List<string> ActivePacts { get; set; } = new();
    /// <summary>Typy paktów zawarte po ostatnim przeliczeniu — działają z połową wartości.</summary>
    public List<string> HalfPacts { get; set; } = new();
}

public class SetPactDto
{
    public int TargetKingdomId { get; set; }
    /// <summary>Typ paktu obronnego: Magiczny | Wojskowy | Zlodziejski.</summary>
    public string PactType { get; set; } = "Wojskowy";
    /// <summary>true = zawrzyj pakt tego typu, false = zerwij.</summary>
    public bool Active { get; set; }
}

public class SetTradePactDto
{
    /// <summary>true = włącz pakt handlowy, false = zerwij.</summary>
    public bool Enabled { get; set; }
}
