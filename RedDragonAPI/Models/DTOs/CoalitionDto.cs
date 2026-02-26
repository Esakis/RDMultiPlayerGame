namespace RedDragonAPI.Models.DTOs;

public class CoalitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public int? LeaderKingdomId { get; set; }
    public string? LeaderName { get; set; }
    public int MemberCount { get; set; }
    public int MaxMembers { get; set; }
    public decimal PSOProgress { get; set; }
    public List<KingdomSummaryDto> Members { get; set; } = new();
}

public class CreateCoalitionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Tag { get; set; }
}

public class JoinCoalitionDto
{
    public int CoalitionId { get; set; }
}

public class AppointMainCommanderDto
{
    public int KingdomId { get; set; }
}
