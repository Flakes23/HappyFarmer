using System.Text.Json;
using Confluent.Kafka;
using HappyFarmer.Shared.Contracts.Events;

namespace HappyFarmer.MarketplaceService.Api.Services;

/// <summary>
/// Consumer đầu tiên trong codebase (BackgroundService) — subscribe topic auth.user-updated.v1,
/// nhóm consumer "marketplace-service-group" (quy ước {service}-group, xem
/// docs/architecture/services/notification-service.md). Kafka at-least-once: xử lý idempotent
/// (ghi đè cùng giá trị nhiều lần vô hại), lỗi từng message chỉ log rồi tiếp tục — đây là cache
/// denormalize không critical, không cần retry/dead-letter cho MVP.
/// </summary>
public class UserProfileUpdatedConsumer(
    IConsumer<string, string> consumer,
    IServiceScopeFactory scopeFactory,
    ILogger<UserProfileUpdatedConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe(KafkaTopics.UserUpdated);
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private async Task ConsumeLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var evt = JsonSerializer.Deserialize<UserProfileUpdatedEvent>(result.Message.Value);
                if (evt is null) continue;

                // DbContext là scoped, BackgroundService là singleton — tạo 1 scope riêng mỗi
                // message thay vì dùng chung 1 scope sống suốt vòng đời service (tránh DbContext
                // bị dùng lại không thread-safe / change-tracker phình to theo thời gian).
                using var scope = scopeFactory.CreateScope();
                var userSync = scope.ServiceProvider.GetRequiredService<DenormalizedUserSyncService>();
                await userSync.SyncUserAsync(evt.UserId, evt.FullName, evt.AvatarUrl);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi xử lý message {Topic} — bỏ qua, tiếp tục consume.", KafkaTopics.UserUpdated);
            }
        }
    }

    public override void Dispose()
    {
        consumer.Close();
        base.Dispose();
    }
}
