namespace TodoApp.Infrastructure.Kafka;

/// <summary>
/// Cài đặt kết nối Kafka
/// </summary>
public class KafkaSettings
{
    /// <summary>
    /// Danh sách máy chủ Kafka
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";
    
    /// <summary>
    /// Tiền tố cho tên topic
    /// </summary>
    public string TopicPrefix { get; set; } = "todo";
    
    /// <summary>
    /// ID nhóm consumer
    /// </summary>
    public string GroupId { get; set; } = "todo-consumer-group";
    
    /// <summary>
    /// ID nhóm consumer (sử dụng bên trong KafkaConsumer)
    /// </summary>
    public string ConsumerGroupId { get; set; } = "todo-consumer-group";
    
    /// <summary>
    /// Tên topic cho sự kiện Todo
    /// </summary>
    public string TodoEventsTopic { get; set; } = "todo-events";
}