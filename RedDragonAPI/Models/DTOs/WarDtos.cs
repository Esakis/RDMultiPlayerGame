namespace RedDragonAPI.Models.DTOs;

public class WarDto
{
    public int Id { get; set; }
    public int DeclaringCoalitionId { get; set; }
    public string DeclaringName { get; set; } = string.Empty;
    public int TargetCoalitionId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public DateTime DeclaredAt { get; set; }
    public bool IsMyDeclaration { get; set; }
    public int OpponentCoalitionId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
}

public class DeclareWarDto
{
    public int TargetCoalitionId { get; set; }
}
