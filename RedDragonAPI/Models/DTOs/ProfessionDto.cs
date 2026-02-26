namespace RedDragonAPI.Models.DTOs;

public class ProfessionDto
{
    public string ProfessionType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int WorkerCount { get; set; }
    public int NoviceCount { get; set; }
    public int MaxCapacity { get; set; }
    public long ProductionPerTurn { get; set; }
    public decimal NovicePercent { get; set; }
}
