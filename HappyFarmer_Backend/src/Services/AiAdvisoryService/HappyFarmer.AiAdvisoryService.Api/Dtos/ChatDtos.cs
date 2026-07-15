namespace HappyFarmer.AiAdvisoryService.Api.Dtos;

public record CreateChatSessionResponse(int SessionId, DateTime StartedAt);

public record ChatSessionSummaryDto(int Id, string? Title, DateTime StartedAt, DateTime LastActivityAt, string Status);

public record SendChatMessageRequest(string Message);

public record SendChatMessageResponse(int SessionId, string Reply, DateTime Timestamp);

public record ChatMessageDto(int Id, string Sender, string Content, DateTime CreatedAt);

/// <summary>
/// Một lượt hội thoại trong sliding-window context (Redis) — Role là "user" hoặc "assistant",
/// khớp với Role của Anthropic SDK khi build lại messages gửi cho Claude.
/// </summary>
public record ChatTurn(string Role, string Content);
