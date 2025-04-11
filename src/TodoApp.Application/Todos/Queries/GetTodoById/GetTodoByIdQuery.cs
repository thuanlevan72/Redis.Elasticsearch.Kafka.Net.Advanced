using AutoMapper;
using MediatR;
using TodoApp.Domain.Exceptions;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Todos.Queries.GetTodoById;

/// <summary>
/// Query để lấy Todo theo ID
/// </summary>
public class GetTodoByIdQuery : IRequest<Result<TodoDetailDto>>
{
    /// <summary>
    /// ID của Todo cần lấy
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// DTO chi tiết cho Todo
/// </summary>
public class TodoDetailDto
{
    /// <summary>
    /// ID của Todo
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Trạng thái hoàn thành
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Mức độ ưu tiên
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Ngày đến hạn
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Ngày cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Handler xử lý query lấy Todo theo ID
/// </summary>
public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, Result<TodoDetailDto>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Khởi tạo handler với dịch vụ Elasticsearch và mapper
    /// </summary>
    /// <param name="elasticsearchService">Dịch vụ Elasticsearch</param>
    /// <param name="mapper">AutoMapper</param>
    public GetTodoByIdQueryHandler(IElasticsearchService elasticsearchService, IMapper mapper)
    {
        // Lưu trữ dịch vụ Elasticsearch
        _elasticsearchService = elasticsearchService;
        // Lưu trữ mapper
        _mapper = mapper;
    }

    /// <summary>
    /// Xử lý query lấy Todo theo ID
    /// </summary>
    /// <param name="request">Query cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result chứa thông tin chi tiết của Todo</returns>
    public async Task<Result<TodoDetailDto>> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Lấy Todo từ Elasticsearch theo ID
            var todo = await _elasticsearchService.GetDocumentAsync<TodoDocument>("todos", request.Id.ToString());

            // Nếu không tìm thấy, ném ra ngoại lệ
            if (todo == null)
            {
                throw new TodoNotFoundException(request.Id);
            }

            // Map từ TodoDocument sang TodoDetailDto
            var todoDto = new TodoDetailDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            };

            // Trả về kết quả thành công với thông tin chi tiết của Todo
            return Result<TodoDetailDto>.Success(todoDto);
        }
        catch (TodoNotFoundException ex)
        {
            // Nếu không tìm thấy Todo, trả về kết quả thất bại với thông báo lỗi
            return Result<TodoDetailDto>.Failure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            // Nếu có lỗi khác, trả về kết quả thất bại với thông báo lỗi
            return Result<TodoDetailDto>.Failure(new[] { $"Lỗi khi lấy thông tin Todo: {ex.Message}" });
        }
    }
}


