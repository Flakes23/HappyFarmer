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

    /// <summary>Card (giá/tin đăng) chatbot tra được qua function-calling, serialize JSON — chỉ tin nhắn AI mới set.</summary>
    public string? CardsJson { get; set; }
}
