namespace RedDragonAPI.Models.DTOs;

public class DragonStatusDto
{
    public long Dragons { get; set; }
    public long Cap { get; set; }
    public string CapSource { get; set; } = string.Empty;
    public int DracoLevel { get; set; }
    public decimal DracoBonusPct { get; set; }
    public bool HasPortal { get; set; }
    public bool CanSummon { get; set; }
    public string? CannotSummonReason { get; set; }

    /// <summary>Wkład smoków do siły armii: mnożnik (1 + r/(50+r)) oraz dodatek r·100.</summary>
    public decimal PowerMultiplier { get; set; }
    public long FlatAttackBonus { get; set; }
}
