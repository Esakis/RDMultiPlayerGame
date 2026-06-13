namespace RedDragonAPI.Models.DTOs;

public class PpsStatusDto
{
    public bool HasCoalition { get; set; }
    public bool IsBuilding { get; set; }
    public long InvestedBudulec { get; set; }
    public long Cost { get; set; }
    public decimal Percent { get; set; }
    public long CoalitionLand { get; set; }
    public long RequiredLand { get; set; }
    public bool LandThresholdMet { get; set; }
    public bool IsLeader { get; set; }
    public string? Role { get; set; }
    public long MyBudulecStored { get; set; }
}

public class ContributePpsDto
{
    public long Budulec { get; set; }
}
