namespace RedDragonAPI.Models.DTOs;

public class TreasuryDto
{
    public bool HasCoalition { get; set; }
    public long TreasuryGold { get; set; }
    public long TreasuryBudulec { get; set; }
    public bool IsLeader { get; set; }
    public long MyGold { get; set; }
    public long MyBudulecStored { get; set; }
    public bool IsBuildingPps { get; set; }
}

public class TreasuryTransferDto
{
    public long Gold { get; set; }
    public long Budulec { get; set; }
}

public class FundPpsDto
{
    public long Budulec { get; set; }
}
