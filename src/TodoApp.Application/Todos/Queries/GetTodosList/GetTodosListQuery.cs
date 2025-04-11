using AutoMapper;
using MediatR;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Todos.Queries.GetTodosList;

/// <summary>
/// Query để lấy danh sách todos với phân trang
/// </summary>
public class GetTodosListQuery : IRequest<Result<PaginatedList<TodoDto>>>
{
    /// <summary>
    /// Số trang
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Kích thước trang
    /// </summary>
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Lọc theo trạng thái hoàn thành
    /// </summary>
    public bool? IsCompleted { get; set; }
    
    /// <summary>
    /// Lọc theo mức độ ưu tiên
    /// </summary>
    public int? Priority { get; set; }
}

/// <summary>
/// DTO cho Todo
/// </summary>
public class TodoDto
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
/// Handler xử lý query lấy danh sách todos
/// </summary>
public class GetTodosListQueryHandler : IRequestHandler<GetTodosListQuery, Result<PaginatedList<TodoDto>>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Khởi tạo handler với dịch vụ Elasticsearch và mapper
    /// </summary>
    /// <param name="elasticsearchService">Dịch vụ Elasticsearch</param>
    /// <param name="mapper">AutoMapper</param>
    public GetTodosListQueryHandler(IElasticsearchService elasticsearchService, IMapper mapper)
    {
        // Lưu trữ dịch vụ Elasticsearch
        _elasticsearchService = elasticsearchService;
        // Lưu trữ mapper
        _mapper = mapper;
    }

    /// <summary>
    /// Xử lý query lấy danh sách todos
    /// </summary>
    /// <param name="request">Query cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result chứa danh sách todos phân trang</returns>
    public async Task<Result<PaginatedList<TodoDto>>> Handle(GetTodosListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Xây dựng truy vấn dựa trên các bộ lọc
            string query = BuildFilterQuery(request.IsCompleted, request.Priority);

            // Tìm kiếm todos trong Elasticsearch
            var (items, totalCount) = await _elasticsearchService.SearchAsync<TodoDocument>(
                "todos", 
                query,
                request.PageNumber, 
                request.PageSize);

            // Map kết quả từ TodoDocument sang TodoDto
            var todoDtos = items.Select(todo => new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            });

            // Tạo danh sách phân trang từ kết quả
            var paginatedList = PaginatedList<TodoDto>.Create(
                todoDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            // Trả về kết quả thành công với danh sách phân trang
            return Result<PaginatedList<TodoDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, trả về kết quả thất bại với thông báo lỗi
            return Result<PaginatedList<TodoDto>>.Failure(new[] { $"Lỗi khi lấy danh sách Todos: {ex.Message}" });
        }
    }

    /// <summary>
    /// Xây dựng truy vấn tìm kiếm dựa trên các bộ lọc
    /// </summary>
    /// <param name="isCompleted">Lọc theo trạng thái hoàn thành</param>
    /// <param name="priority">Lọc theo mức độ ưu tiên</param>
    /// <returns>Truy vấn tìm kiếm</returns>
    private string BuildFilterQuery(bool? isCompleted, int? priority)
    {
        var filters = new List<string>();

        // Thêm bộ lọc cho trạng thái hoàn thành nếu có
        if (isCompleted.HasValue)
        {
            filters.Add($"isCompleted:{isCompleted.Value.ToString().ToLower()}");
        }

        // Thêm bộ lọc cho mức độ ưu tiên nếu có
        if (priority.HasValue)
        {
            filters.Add($"priority:{priority.Value}");
        }

        // Nếu không có bộ lọc, trả về truy vấn khớp với tất cả
        if (!filters.Any())
        {
            return "*";
        }

        // Kết hợp các bộ lọc với toán tử AND
        return string.Join(" AND ", filters);
    }
}
