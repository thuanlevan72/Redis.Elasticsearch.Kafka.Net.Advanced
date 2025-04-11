namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Interface cho dịch vụ gửi sự kiện đến Kafka
/// </summary>
public interface IKafkaProducer : IDisposable
{
    /// <summary>
    /// Gửi message đến một topic cụ thể
    /// </summary>
    /// <typeparam name="TKey">Kiểu của khóa</typeparam>
    /// <typeparam name="TValue">Kiểu của giá trị</typeparam>
    /// <param name="topic">Tên topic</param>
    /// <param name="key">Khóa của message</param>
    /// <param name="value">Giá trị của message</param>
    /// <param name="cancellationToken">Token hủy</param>
    Task ProduceAsync<TKey, TValue>(string topic, TKey key, TValue value, CancellationToken cancellationToken = default);
}