using System.Security.Claims;
using HappyFarmer.MarketplaceService.Api.Data;
using HappyFarmer.MarketplaceService.Api.Dtos;
using HappyFarmer.MarketplaceService.Api.Entities;
using HappyFarmer.MarketplaceService.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HappyFarmer.MarketplaceService.Api.Controllers;

[ApiController]
[Route("api/marketplace/my-interests")]
[Authorize]
public class InterestsController(MarketplaceDbContext db, IHubContext<ChatHub> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InterestResponse>>> GetMyInterests()
    {
        var userId = GetCurrentUserId();
        var interests = await db.Interests
            .Where(i => i.InitiatorUserId == userId || i.TargetUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(interests.Select(InterestResponse.FromEntity));
    }

    [HttpGet("{id:int}/messages")]
    public async Task<ActionResult<MessageHistoryResponse>> GetMessages(
        int id, [FromQuery] int? beforeId, [FromQuery] int take = 50)
    {
        var interest = await EnsureParticipantAsync(id);
        if (interest is null) return NotFound();

        var query = db.Messages.Where(m => m.InterestId == id);
        if (beforeId is not null) query = query.Where(m => m.Id < beforeId);

        var page = await query.OrderByDescending(m => m.Id).Take(take + 1).ToListAsync();
        var hasMore = page.Count > take;
        page = page.Take(take).ToList();
        page.Reverse();

        var messages = page.Select(MessageResponse.FromEntity).ToList();
        if (beforeId is null && !hasMore)
        {
            messages.Insert(0, MessageResponse.FromInterestSeed(interest));
        }

        return Ok(new MessageHistoryResponse(messages, hasMore));
    }

    [HttpPost("{id:int}/messages")]
    public async Task<ActionResult<MessageResponse>> SendMessage(int id, SendMessageRequest request)
    {
        var interest = await EnsureParticipantAsync(id);
        if (interest is null) return NotFound();

        var message = new Message
        {
            InterestId = id,
            SenderUserId = GetCurrentUserId()!.Value,
            Body = request.Body,
        };
        db.Messages.Add(message);

        if (interest.Status == InterestStatus.Pending && message.SenderUserId == interest.TargetUserId)
        {
            interest.Status = InterestStatus.Responded;
        }

        await db.SaveChangesAsync();

        var response = MessageResponse.FromEntity(message);
        await hubContext.Clients.Group(ChatHub.GroupName(id)).SendAsync("ReceiveMessage", response);

        return Ok(response);
    }

    private async Task<Interest?> EnsureParticipantAsync(int interestId)
    {
        var userId = GetCurrentUserId();
        var interest = await db.Interests.FirstOrDefaultAsync(i => i.Id == interestId);
        if (interest is null) return null;
        if (interest.InitiatorUserId != userId && interest.TargetUserId != userId) return null;
        return interest;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(idClaim, out var id) ? id : null;
    }
}
