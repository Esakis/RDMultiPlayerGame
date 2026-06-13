namespace RedDragonAPI.Models.DTOs;

public class ElectionCandidateDto
{
    public int KingdomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Votes { get; set; }
    public bool IsImperator { get; set; }
    public bool IsMyVote { get; set; }
}

public class ElectionDto
{
    public bool HasCoalition { get; set; }
    public int? CurrentImperatorId { get; set; }
    public string? CurrentImperatorName { get; set; }
    public int? MyVoteKingdomId { get; set; }
    public int TotalMembers { get; set; }
    public List<ElectionCandidateDto> Candidates { get; set; } = new();
}

public class VoteImperatorDto
{
    public int CandidateKingdomId { get; set; }
}
