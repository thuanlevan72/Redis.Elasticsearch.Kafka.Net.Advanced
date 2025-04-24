using MediatR;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Models;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Todos.Commands.CreateTodo;

namespace TodoApp.Application.Todos.EventHandlers;

/// <summary>
/// Handler xử lý sự kiện khi Todo được tạo
/// </summary>
public class ProductDeletedEventHandler : INotificationHandler<ProductDeletedNotification>
{
    // private readonly IKafkaProducer _kafkaProducer;
    private readonly IElasticsearchService _elasticsearchService;
    private string _index;

    /// <summary>
    /// Khởi tạo handler với repository tìm kiếm
    /// </summary>
    /// <param name="todoSearchRepository">Repository tìm kiếm Todo</param>
    public ProductDeletedEventHandler(IElasticsearchService elasticsearchService)
    {
        // Lưu trữ repository tìm kiếm
        _elasticsearchService = elasticsearchService;
        _index = "products";
    }

    /// <summary>
    /// Xử lý thông báo sự kiện Todo được tạo
    /// </summary>
    /// <param name="notification">Thông báo sự kiện</param>
    /// <param name="cancellationToken">Token hủy</param>
    public async Task Handle(ProductDeletedNotification notification, CancellationToken cancellationToken)
    {
        // Log thông tin về Todo đã được tạo
        Console.WriteLine($"Đang xử lý xóa products: {notification.IGuid}");

        try
        {
            await _elasticsearchService.DeleteDocumentAsync(_index,  notification.IGuid.ToString());
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có
            Console.WriteLine($"Lỗi khi đồng bộ Products vào Elasticsearch: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// Thông báo sự kiện khi Todo được tạo
/// </summary>
public class ProductDeletedNotification : INotification
{
    /// <summary>
    /// Sự kiện ProductId phải xóa
    /// </summary>
    public Guid IGuid { get; }

    /// <summary>
    /// Khởi tạo thông báo với sự kiện
    /// </summary>
    /// <param name="todoCreatedEvent">Sự kiện Todo đã tạo</param>
    public ProductDeletedNotification(Guid productId)
    {
        // Lưu trữ sự kiện
        IGuid = productId;
    }
}

