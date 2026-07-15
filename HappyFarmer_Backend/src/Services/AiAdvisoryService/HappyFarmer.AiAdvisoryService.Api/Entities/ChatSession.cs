namespace HappyFarmer.AiAdvisoryService.Api.Entities;

public enum ChatSessionStatus
{
    Active,
    Ended
}

public class ChatSession
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public string? Title { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Active;
}
