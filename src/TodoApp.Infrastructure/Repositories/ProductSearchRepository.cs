using Microsoft.Extensions.Logging;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Models;
using Dimensions = TodoApp.Application.Common.Models.Dimensions;
using ManufacturerInfo = TodoApp.Application.Common.Models.ManufacturerInfo;

namespace TodoApp.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý tìm kiếm Todo trong Elasticsearch
/// </summary>
public class ProductSearchRepository : IProductSearchRepository
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<ProductSearchRepository> _logger;
    private const string IndexName = "products";

    /// <summary>
    /// Khởi tạo repository với dịch vụ Elasticsearch và logger
    /// </summary>
    /// <param name="elasticsearchService">Dịch vụ Elasticsearch</param>
    /// <param name="logger">Logger</param>
    public ProductSearchRepository(
        IElasticsearchService elasticsearchService,
        ILogger<ProductSearchRepository> logger)
    {
        // Lưu trữ dịch vụ Elasticsearch
        _elasticsearchService = elasticsearchService;
        // Lưu trữ logger
        _logger = logger;
    }
    

    public Task<(IEnumerable<Product> Items, long TotalCount)> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Đồng bộ product sang productDocument index = products
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    public async Task IndexAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Đồng bộ Product có ID {ProductId} vào Elasticsearch", product.Id);

            // Chuyển đổi Todo thành TodoDocument
            var document = MapToDocument(product);

            // Lưu trữ vào Elasticsearch
            var result = await _elasticsearchService.IndexDocumentAsync(
                IndexName, 
                document, 
                product.Id.ToString());

            // Log kết quả
            if (result)
            {
                _logger.LogInformation("Đã đồng bộ Product có ID {ProductId} vào Elasticsearch thành công", product.Id);
            }
            else
            {
                _logger.LogWarning("Không thể đồng bộ Product có ID {ProductId} vào Elasticsearch", product.Id);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi đồng bộ Product có ID {TodoId} vào Elasticsearch", product.Id);
            throw;
        }
    }

    /// <summary>
    /// Xóa một todo khỏi Elasticsearch
    /// </summary>
    /// <param name="todoId">ID của todo cần xóa</param>
    public async Task DeleteAsync(Guid todoId)
    {
        try
        {
            _logger.LogInformation("Xóa Product có ID {ProductId} khỏi Elasticsearch", todoId);

            // Xóa khỏi Elasticsearch
            var result = await _elasticsearchService.DeleteDocumentAsync(
                IndexName, 
                todoId.ToString());

            // Log kết quả
            if (result)
            {
                _logger.LogInformation("Đã xóa Product có ID {ProductId} khỏi Elasticsearch thành công", todoId);
            }
            else
            {
                _logger.LogWarning("Không thể xóa Product có ID {ProductId} khỏi Elasticsearch", todoId);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi xóa Todo có ID {ProductId} khỏi Elasticsearch", todoId);
            throw;
        }
    }

    /// <summary>
    ///  Chuyển Product sang ProductDocument
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private ProductDocument MapToDocument(Product product)
    {
        try
        {
          return new ProductDocument()
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                Category = product.Category,
                Material = product.Material,
                ManufacturingDate = product.ManufacturingDate,
                Status = product.Status,
                Dimensions = new Dimensions()
                {
                    Height = product.Dimensions.Height,
                    Length = product.Dimensions.Length,
                    Width = product.Dimensions.Width
                },
                Tags = product.Tags,
                Manufacturer = new ManufacturerInfo()
                {
                    Country = product.Manufacturer.Country,
                    Name = product.Manufacturer.Name
                }
            };
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi chuyển đổi Product sang ProductDocument");
            throw;
        }
    }
}
