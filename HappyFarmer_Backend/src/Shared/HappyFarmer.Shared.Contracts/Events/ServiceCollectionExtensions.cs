using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HappyFarmer.Shared.Contracts.Events;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký IProducer&lt;string,string&gt; dùng chung (singleton, giữ 1 kết nối lâu dài giống
    /// IConnectionMultiplexer của Redis). Publish JSON string, không cần key/partition riêng vì
    /// khối lượng sự kiện ở đây rất nhỏ.
    /// </summary>
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                // Mặc định librdkafka đợi tới 5 phút (message.timeout.ms) trước khi báo lỗi delivery
                // — quá lâu cho publish "best-effort" không được chặn request của người dùng. Hạ
                // xuống vài giây để ProduceAsync fail nhanh khi Kafka không kết nối được, thay vì
                // treo cả request PUT /api/auth/me.
                MessageTimeoutMs = 5000,
                SocketTimeoutMs = 5000,
            };
            return new ProducerBuilder<string, string>(config).Build();
        });

        return services;
    }

    /// <summary>
    /// Đăng ký IConsumer&lt;string,string&gt; dùng chung — không dùng trực tiếp qua DI theo nghĩa
    /// scoped bình thường, chỉ làm nguồn kết nối cho 1 BackgroundService tự quản lý vòng đời
    /// (subscribe/consume loop) — xem UserProfileUpdatedConsumer ở Marketplace Service.
    /// </summary>
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration, string groupId)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
            };
            return new ConsumerBuilder<string, string>(config).Build();
        });

        return services;
    }
}
