using AutoMapper;
using MediatR;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Todos.Queries.SearchTodos;

/// <summary>
/// Query để tìm kiếm todos theo từ khóa
/// </summary>
public class SearchTodosQuery : IRequest<Result<PaginatedList<TodoSearchResultDto>>>
{
    /// <summary>
    /// Từ khóa tìm kiếm
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;
    
    /// <summary>
    /// Số trang
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Kích thước trang
    /// </summary>
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO kết quả tìm kiếm Todo
/// </summary>
public class TodoSearchResultDto
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
}

/// <summary>
/// Handler xử lý query tìm kiếm todos
/// </summary>
public class SearchTodosQueryHandler : IRequestHandler<SearchTodosQuery, Result<PaginatedList<TodoSearchResultDto>>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Khởi tạo handler với dịch vụ Elasticsearch và mapper
    /// </summary>
    /// <param name="elasticsearchService">Dịch vụ Elasticsearch</param>
    /// <param name="mapper">AutoMapper</param>
    public SearchTodosQueryHandler(IElasticsearchService elasticsearchService, IMapper mapper)
    {
        // Lưu trữ dịch vụ Elasticsearch
        _elasticsearchService = elasticsearchService;
        // Lưu trữ mapper
        _mapper = mapper;
    }

    /// <summary>
    /// Xử lý query tìm kiếm todos
    /// </summary>
    /// <param name="request">Query cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result chứa kết quả tìm kiếm phân trang</returns>
    public async Task<Result<PaginatedList<TodoSearchResultDto>>> Handle(SearchTodosQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Kiểm tra từ khóa tìm kiếm, nếu rỗng thì tìm tất cả
            string searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? "*" : request.SearchTerm;

            // Tìm kiếm todos trong Elasticsearch
            var (items, totalCount) = await _elasticsearchService.SearchAsync<TodoDocument>(
                "todos", 
                searchTerm,
                request.PageNumber, 
                request.PageSize);

            // Map kết quả từ TodoDocument sang TodoSearchResultDto
            var todoResults = items.Select(todo => new TodoSearchResultDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                Priority = todo.Priority,
                DueDate = todo.DueDate
            });

            // Tạo danh sách phân trang từ kết quả
            var paginatedList = PaginatedList<TodoSearchResultDto>.Create(
                todoResults,
                totalCount,
                request.PageNumber,
                request.PageSize);

            // Trả về kết quả thành công với danh sách phân trang
            return Result<PaginatedList<TodoSearchResultDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, trả về kết quả thất bại với thông báo lỗi
            return Result<PaginatedList<TodoSearchResultDto>>.Failure(new[] { $"Lỗi khi tìm kiếm Todos: {ex.Message}" });
        }
    }
}
