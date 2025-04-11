using MediatR;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Todos.Commands.CreateTodo;

/// <summary>
/// DTO cho command tạo Todo mới
/// </summary>
public class CreateTodoCommand : IRequest<Result<Guid>>
{
    /// <summary>
    /// Tiêu đề của Todo
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả của Todo
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Mức độ ưu tiên của Todo
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Ngày đến hạn của Todo
    /// </summary>
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Handler xử lý command tạo Todo mới
/// </summary>
public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, Result<Guid>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IKafkaProducer _kafkaProducer;

    /// <summary>
    /// Khởi tạo handler với repository và Kafka producer
    /// </summary>
    /// <param name="todoRepository">Repository xử lý Todo</param>
    /// <param name="kafkaProducer">Producer gửi message đến Kafka</param>
    public CreateTodoCommandHandler(ITodoRepository todoRepository, IKafkaProducer kafkaProducer)
    {
        // Lưu trữ repository
        _todoRepository = todoRepository;
        // Lưu trữ Kafka producer
        _kafkaProducer = kafkaProducer;
    }

    /// <summary>
    /// Xử lý command tạo Todo mới
    /// </summary>
    /// <param name="request">Command cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result chứa ID của Todo mới tạo</returns>
    public async Task<Result<Guid>> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Tạo entity Todo mới từ request
            var todo = new Todo(
                request.Title,
                request.Description,
                request.Priority,
                request.DueDate
            );

            // Lưu Todo vào database
            await _todoRepository.AddAsync(todo);
            await _todoRepository.SaveChangesAsync();

            // Tạo message sự kiện Todo đã tạo
            var todoCreatedEvent = new TodoCreatedEvent
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt
            };

            // Gửi sự kiện đến Kafka
            await _kafkaProducer.ProduceAsync("todo-events", todo.Id.ToString(), todoCreatedEvent, cancellationToken);

            // Trả về kết quả thành công với ID của Todo
            return Result<Guid>.Success(todo.Id);
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, trả về kết quả thất bại với thông báo lỗi
            return Result<Guid>.Failure(new[] { $"Lỗi khi tạo Todo: {ex.Message}" });
        }
    }
}

/// <summary>
/// Event khi Todo được tạo
/// </summary>
public class TodoCreatedEvent
{
    /// <summary>
    /// ID của Todo
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tiêu đề của Todo
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả của Todo
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Mức độ ưu tiên của Todo
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Ngày đến hạn của Todo
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Trạng thái hoàn thành của Todo
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Ngày tạo Todo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
