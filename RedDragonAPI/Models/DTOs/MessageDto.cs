namespace RedDragonAPI.Models.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}

public class SendMessageDto
{
    public int ReceiverKingdomId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
