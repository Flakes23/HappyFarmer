namespace HappyFarmer.Shared.Contracts.Events;

/// <summary>Bind từ section "Kafka" trong appsettings.</summary>
public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; set; }
}
