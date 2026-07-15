namespace HappyFarmer.AiAdvisoryService.Api.Entities;

public enum ChatSender
{
    User,
    AI
}

public class ChatMessage
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
