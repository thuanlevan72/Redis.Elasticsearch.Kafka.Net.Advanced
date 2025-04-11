using MediatR;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Todos.Commands.DeleteTodo;

namespace TodoApp.Application.Todos.EventHandlers;

/// <summary>
/// Handler xử lý sự kiện khi Todo bị xóa
/// </summary>
public class TodoDeletedEventHandler : INotificationHandler<TodoDeletedNotification>
{
    private readonly ITodoSearchRepository _todoSearchRepository;

    /// <summary>
    /// Khởi tạo handler với repository tìm kiếm
    /// </summary>
    /// <param name="todoSearchRepository">Repository tìm kiếm Todo</param>
    public TodoDeletedEventHandler(ITodoSearchRepository todoSearchRepository)
    {
        // Lưu trữ repository tìm kiếm
        _todoSearchRepository = todoSearchRepository;
    }

    /// <summary>
    /// Xử lý thông báo sự kiện Todo bị xóa
    /// </summary>
    /// <param name="notification">Thông báo sự kiện</param>
    /// <param name="cancellationToken">Token hủy</param>
    public async Task Handle(TodoDeletedNotification notification, CancellationToken cancellationToken)
    {
        // Log thông tin về Todo đã bị xóa
        Console.WriteLine($"Đang xử lý sự kiện Todo đã xóa: {notification.TodoDeletedEvent.Id}");

        try
        {
            // Xóa Todo khỏi Elasticsearch
            await _todoSearchRepository.DeleteAsync(notification.TodoDeletedEvent.Id);

            // Log thông tin thành công
            Console.WriteLine($"Đã xóa Todo {notification.TodoDeletedEvent.Id} khỏi Elasticsearch");
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có
            Console.WriteLine($"Lỗi khi xóa Todo khỏi Elasticsearch: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// Thông báo sự kiện khi Todo bị xóa
/// </summary>
public class TodoDeletedNotification : INotification
{
    /// <summary>
    /// Sự kiện Todo đã xóa
    /// </summary>
    public TodoDeletedEvent TodoDeletedEvent { get; }

    /// <summary>
    /// Khởi tạo thông báo với sự kiện
    /// </summary>
    /// <param name="todoDeletedEvent">Sự kiện Todo đã xóa</param>
    public TodoDeletedNotification(TodoDeletedEvent todoDeletedEvent)
    {
        // Lưu trữ sự kiện
        TodoDeletedEvent = todoDeletedEvent;
    }
}
