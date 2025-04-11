using MediatR;
using TodoApp.Domain.Exceptions;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Todos.Commands.DeleteTodo;

/// <summary>
/// Command để xóa Todo
/// </summary>
public class DeleteTodoCommand : IRequest<Result>
{
    /// <summary>
    /// ID của Todo cần xóa
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Handler xử lý command xóa Todo
/// </summary>
public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, Result>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IKafkaProducer _kafkaProducer;

    /// <summary>
    /// Khởi tạo handler với repository và Kafka producer
    /// </summary>
    /// <param name="todoRepository">Repository xử lý Todo</param>
    /// <param name="kafkaProducer">Producer gửi message đến Kafka</param>
    public DeleteTodoCommandHandler(ITodoRepository todoRepository, IKafkaProducer kafkaProducer)
    {
        // Lưu trữ repository
        _todoRepository = todoRepository;
        // Lưu trữ Kafka producer
        _kafkaProducer = kafkaProducer;
    }

    /// <summary>
    /// Xử lý command xóa Todo
    /// </summary>
    /// <param name="request">Command cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result thông báo kết quả</returns>
    public async Task<Result> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
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

            // Xóa Todo khỏi database
            await _todoRepository.DeleteAsync(todo);
            await _todoRepository.SaveChangesAsync();

            // Tạo message sự kiện Todo đã xóa
            var todoDeletedEvent = new TodoDeletedEvent
            {
                Id = request.Id
            };

            // Gửi sự kiện đến Kafka
            await _kafkaProducer.ProduceAsync("todo-events", request.Id.ToString(), todoDeletedEvent, cancellationToken);

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
            return Result.Failure(new[] { $"Lỗi khi xóa Todo: {ex.Message}" });
        }
    }
}

/// <summary>
/// Event khi Todo bị xóa
/// </summary>
public class TodoDeletedEvent
{
    /// <summary>
    /// ID của Todo bị xóa
    /// </summary>
    public Guid Id { get; set; }
}
