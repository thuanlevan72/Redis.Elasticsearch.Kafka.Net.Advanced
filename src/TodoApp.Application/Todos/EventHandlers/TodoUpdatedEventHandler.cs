using MediatR;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Todos.Commands.UpdateTodo;

namespace TodoApp.Application.Todos.EventHandlers;

/// <summary>
/// Handler xử lý sự kiện khi Todo được cập nhật
/// </summary>
public class TodoUpdatedEventHandler : INotificationHandler<TodoUpdatedNotification>
{
    private readonly ITodoSearchRepository _todoSearchRepository;

    /// <summary>
    /// Khởi tạo handler với repository tìm kiếm
    /// </summary>
    /// <param name="todoSearchRepository">Repository tìm kiếm Todo</param>
    public TodoUpdatedEventHandler(ITodoSearchRepository todoSearchRepository)
    {
        // Lưu trữ repository tìm kiếm
        _todoSearchRepository = todoSearchRepository;
    }

    /// <summary>
    /// Xử lý thông báo sự kiện Todo được cập nhật
    /// </summary>
    /// <param name="notification">Thông báo sự kiện</param>
    /// <param name="cancellationToken">Token hủy</param>
    public async Task Handle(TodoUpdatedNotification notification, CancellationToken cancellationToken)
    {
        // Log thông tin về Todo đã được cập nhật
        Console.WriteLine($"Đang xử lý sự kiện Todo đã cập nhật: {notification.TodoUpdatedEvent.Id}");

        try
        {
            // Tạo entity Todo từ sự kiện
            var todo = new Todo(
                notification.TodoUpdatedEvent.Title,
                notification.TodoUpdatedEvent.Description,
                notification.TodoUpdatedEvent.Priority,
                notification.TodoUpdatedEvent.DueDate
            );

            // Phản chiếu ID, IsCompleted và UpdatedAt từ sự kiện
            // Lưu ý: Trong môi trường thực tế, bạn nên sử dụng Reflection hoặc tạo constructor đặc biệt
            // Đây là cách tiếp cận đơn giản để minh họa
            var todoType = typeof(Todo);
            var idProperty = todoType.GetProperty("Id");
            var isCompletedProperty = todoType.GetProperty("IsCompleted");
            var updatedAtProperty = todoType.GetProperty("UpdatedAt");

            if (idProperty != null && isCompletedProperty != null && updatedAtProperty != null)
            {
                idProperty.SetValue(todo, notification.TodoUpdatedEvent.Id);
                isCompletedProperty.SetValue(todo, notification.TodoUpdatedEvent.IsCompleted);
                updatedAtProperty.SetValue(todo, notification.TodoUpdatedEvent.UpdatedAt);
            }

            // Đồng bộ Todo vào Elasticsearch
            await _todoSearchRepository.IndexAsync(todo);

            // Log thông tin thành công
            Console.WriteLine($"Đã đồng bộ Todo {notification.TodoUpdatedEvent.Id} vào Elasticsearch");
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có
            Console.WriteLine($"Lỗi khi đồng bộ Todo vào Elasticsearch: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// Thông báo sự kiện khi Todo được cập nhật
/// </summary>
public class TodoUpdatedNotification : INotification
{
    /// <summary>
    /// Sự kiện Todo đã cập nhật
    /// </summary>
    public TodoUpdatedEvent TodoUpdatedEvent { get; }

    /// <summary>
    /// Khởi tạo thông báo với sự kiện
    /// </summary>
    /// <param name="todoUpdatedEvent">Sự kiện Todo đã cập nhật</param>
    public TodoUpdatedNotification(TodoUpdatedEvent todoUpdatedEvent)
    {
        // Lưu trữ sự kiện
        TodoUpdatedEvent = todoUpdatedEvent;
    }
}
