using AutoMapper;
using MediatR;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Models;

namespace TodoApp.Application.Todos.Queries.ProductQueries;

public class GetProductsListQuery : IRequest<Result<PaginatedList<ProductDocument>>>
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
    /// Tìm kiếm theo tên sảm phẩm
    /// </summary>
    public string? SearchName { get; set; }

    /// <summary>
    /// Tìm kiếm theo description
    /// </summary>
    public string? searchDescription { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<string> searchTags { get; set; } = new List<string>();
}

// <summary>
/// Handler xử lý query lấy danh sách todos
/// </summary>
public class GetProductsListQueryHandler : IRequestHandler<GetProductsListQuery, Result<PaginatedList<ProductDocument>>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Khởi tạo handler với dịch vụ Elasticsearch và mapper
    /// </summary>
    /// <param name="elasticsearchService">Dịch vụ Elasticsearch</param>
    /// <param name="mapper">AutoMapper</param>
    public GetProductsListQueryHandler(IElasticsearchService elasticsearchService, IMapper mapper)
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
    public async Task<Result<PaginatedList<ProductDocument>>> Handle(GetProductsListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Xây dựng truy vấn dựa trên các bộ lọc
            string query = BuildFilterQuery(request.SearchName, request.searchDescription, request.searchTags);

            // Tìm kiếm todos trong Elasticsearch
            var (items, totalCount) = await _elasticsearchService.SearchAsync<ProductDocument>(
                "products",
                query,
                request.PageNumber,
                request.PageSize);
            


            // Tạo danh sách phân trang từ kết quả
            var paginatedList = PaginatedList<ProductDocument>.Create(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            // Trả về kết quả thành công với danh sách phân trang
            return Result<PaginatedList<ProductDocument>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, trả về kết quả thất bại với thông báo lỗi
            return Result<PaginatedList<ProductDocument>>.Failure(
                new[] { $"Lỗi khi lấy danh sách Todos: {ex.Message}" });
        }
    }

    private string BuildFilterQuery(string? SearchName, string? searchDescription, List<string>? searchTags)
    {
        var filters = new List<string>();

// Tìm kiếm theo tên sản phẩm
        if (!string.IsNullOrWhiteSpace(SearchName))
        {
            filters.Add($"name:{SearchName}");
        }

// Tìm kiếm theo mô tả
        if (!string.IsNullOrWhiteSpace(searchDescription))
        {
            filters.Add($"description:{searchDescription}");
        }

// Tìm kiếm theo tags
        if (searchTags != null && searchTags.Any())
        {
            var tagsFilter = string.Join(" OR ", searchTags.Select(tag => $"tags:{tag}"));
            filters.Add($"({tagsFilter})");
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