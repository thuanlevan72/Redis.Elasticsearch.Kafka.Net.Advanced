using MediatR;
using TodoApp.Domain.Exceptions;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Todos.Commands.UpdateTodo;

/// <summary>
/// Command để cập nhật Todo
/// </summary>
public class UpdateTodoCommand : IRequest<Result>
{
    /// <summary>
    /// ID của Todo cần cập nhật
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tiêu đề mới
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả mới
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Trạng thái hoàn thành mới
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Mức độ ưu tiên mới
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Ngày đến hạn mới
    /// </summary>
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Handler xử lý command cập nhật Todo
/// </summary>
public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, Result>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IKafkaProducer _kafkaProducer;

    /// <summary>
    /// Khởi tạo handler với repository và Kafka producer
    /// </summary>
    /// <param name="todoRepository">Repository xử lý Todo</param>
    /// <param name="kafkaProducer">Producer gửi message đến Kafka</param>
    public UpdateTodoCommandHandler(ITodoRepository todoRepository, IKafkaProducer kafkaProducer)
    {
        // Lưu trữ repository
        _todoRepository = todoRepository;
        // Lưu trữ Kafka producer
        _kafkaProducer = kafkaProducer;
    }

    /// <summary>
    /// Xử lý command cập nhật Todo
    /// </summary>
    /// <param name="request">Command cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result thông báo kết quả</returns>
    public async Task<Result> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Tìm Todo theo ID
            var todo = await _todoRepository.GetByIdAsync(request.Id);

            // Nếu không tìm thấy, ném ra ngoại lệ
            if (todo == null)
            {
                throw new TodoNotFoundException(request.Id);
            }

            // Cập nhật thông tin Todo
            todo.Update(
                request.Title,
                request.Description,
                request.Priority,
                request.DueDate
            );

            // Cập nhật trạng thái hoàn thành
            if (request.IsCompleted)
            {
                todo.MarkAsCompleted();
            }
            else
            {
                todo.MarkAsIncomplete();
            }

            // Lưu Todo vào database
            await _todoRepository.UpdateAsync(todo);
            await _todoRepository.SaveChangesAsync();

            // Tạo message sự kiện Todo đã cập nhật
            var todoUpdatedEvent = new TodoUpdatedEvent
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
                UpdatedAt = todo.UpdatedAt
            };

            // Gửi sự kiện đến Kafka
            await _kafkaProducer.ProduceAsync("todo-events", todo.Id.ToString(), todoUpdatedEvent, cancellationToken);

            // Trả về kết quả thành công
            return Result.Success();
        }
        catch (TodoNotFoundException ex)
        {
            // Nếu không tìm thấy Todo, trả về kết quả thất bại với thông báo lỗi
            return Result.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            // Nếu có lỗi khác, trả về kết quả thất bại với thông báo lỗi
            return Result.Failure(new[] { $"Lỗi khi cập nhật Todo: {ex.Message}" });
        }
    }
}

/// <summary>
/// Event khi Todo được cập nhật
/// </summary>
public class TodoUpdatedEvent
{
    /// <summary>
    /// ID của Todo
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tiêu đề mới
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả mới
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Mức độ ưu tiên mới
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Ngày đến hạn mới
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Trạng thái hoàn thành mới
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
