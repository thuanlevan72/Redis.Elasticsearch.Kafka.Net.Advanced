using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Infrastructure.Kafka;

/// <summary>
/// Dịch vụ gửi sự kiện đến Kafka
/// </summary>
public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    /// <summary>
    /// Khởi tạo producer với cài đặt và logger
    /// </summary>
    /// <param name="settings">Cài đặt Kafka</param>
    /// <param name="logger">Logger</param>
    public KafkaProducer(IOptions<KafkaSettings> settings, ILogger<KafkaProducer> logger)
    {
        // Lưu trữ logger
        _logger = logger;

        // Tạo cấu hình cho producer
        var config = new ProducerConfig
        {
            // Danh sách máy chủ Kafka
            BootstrapServers = settings.Value.BootstrapServers,
            // Cấu hình thêm để đảm bảo tin nhắn được gửi đi
            Acks = Acks.All,
            // Số lần thử lại nếu gặp lỗi
            MessageSendMaxRetries = 3,
            // Thời gian chờ giữa các lần thử lại
            RetryBackoffMs = 1000
        };

        // Khởi tạo producer
        _producer = new ProducerBuilder<string, string>(config).Build();
        
        // Log thông tin khởi tạo thành công
        _logger.LogInformation("Kafka Producer đã được khởi tạo với BootstrapServers: {Servers}", settings.Value.BootstrapServers);
    }

    /// <summary>
    /// Gửi message đến một topic cụ thể
    /// </summary>
    /// <typeparam name="TKey">Kiểu của khóa</typeparam>
    /// <typeparam name="TValue">Kiểu của giá trị</typeparam>
    /// <param name="topic">Tên topic</param>
    /// <param name="key">Khóa của message</param>
    /// <param name="value">Giá trị của message</param>
    /// <param name="cancellationToken">Token hủy</param>
    public async Task ProduceAsync<TKey, TValue>(string topic, TKey key, TValue value, CancellationToken cancellationToken = default)
    {
        try
        {
            // Log thông tin chuẩn bị gửi message
            _logger.LogInformation("Chuẩn bị gửi message đến topic {Topic} với key {Key}", topic, key);

            // Chuyển đổi khóa và giá trị thành chuỗi JSON
            var keyString = key?.ToString() ?? string.Empty;
            var valueString = JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                // Bỏ qua các thuộc tính có giá trị null
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                // Format JSON dễ đọc (trong môi trường production, có thể tắt cài đặt này để giảm kích thước)
                WriteIndented = true
            });

            // Tạo message
            var message = new Message<string, string>
            {
                Key = keyString,
                Value = valueString
            };

            // Gửi message đến Kafka
            var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken);

            // Log thông tin gửi thành công
            _logger.LogInformation(
                "Đã gửi message thành công đến topic {Topic}, partition {Partition}, offset {Offset}",
                deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            // Log lỗi khi gửi message
            _logger.LogError(ex, 
                "Lỗi khi gửi message đến topic {Topic}: {Error}",
                topic, ex.Error.Reason);
            
            // Ném lại ngoại lệ để caller có thể xử lý
            throw;
        }
        catch (Exception ex)
        {
            // Log lỗi không mong đợi
            _logger.LogError(ex, 
                "Lỗi không mong đợi khi gửi message đến topic {Topic}",
                topic);
            
            // Ném lại ngoại lệ để caller có thể xử lý
            throw;
        }
    }

    /// <summary>
    /// Giải phóng tài nguyên khi đối tượng bị hủy
    /// </summary>
    public void Dispose()
    {
        // Giải phóng producer
        _producer?.Dispose();
        
        // Log thông tin giải phóng
        _logger.LogInformation("Kafka Producer đã được giải phóng");
    }
}
