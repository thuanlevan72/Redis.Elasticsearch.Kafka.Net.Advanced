using Microsoft.AspNetCore.Mvc;
using MediatR;
using TodoApp.Application.Todos.Commands;
using TodoApp.Application.Todos.Queries.GetTodoById;
using TodoApp.Application.Todos.Queries.GetTodosList;
using TodoApp.Application.Todos.Queries.ProductQueries;
using TodoApp.Application.Todos.Queries.SearchTodos;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Data;
using Dimensions = TodoApp.Application.Common.Models.Dimensions;

namespace TodoApp.ReadApi.Controllers;

/// <summary>
/// API controller cho thao tác đọc với Todo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Khởi tạo controller với mediator và logger
    /// </summary>
    /// <param name="mediator">Mediator để gửi query</param>
    /// <param name="logger">Logger</param>
    public ProductsController(IMediator mediator, ILogger<ProductsController> logger, ApplicationDbContext context)
    {
        // Lưu trữ mediator
        _mediator = mediator;
        // Lưu trữ logger
        _logger = logger;
        _context = context;
    }
    /// Thêm danh sách product ngẫu nhiên
    // <summary>
    /// Lấy thông tin Todo theo ID
    /// </summary>
    /// <param name="id">ID của Todo cần lấy</param>
    /// <returns>Thông tin chi tiết Todo</returns>
    [HttpPost("add-product-ramdom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductsRamdom([FromBody] CreateProductCommand command)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu ramdom sản phẩm cho thư viện:");
            
            // Gửi query đến handler
            var result = await _mediator.Send(command);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã lấy thông tin Todo có ID: {TodoId} thành công", result.Data.ToString());
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi trong quá trình tạo data lên alasticsearch có vấn đề");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }
    
    /// Thêm danh sách product ngẫu nhiên
    // <summary>
    /// Lấy thông tin Todo theo ID
    /// </summary>
    /// <param name="id">ID của Todo cần lấy</param>
    /// <returns>Thông tin chi tiết Todo</returns>
    [HttpGet("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductsRamdom(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] string? searchDescription = null,
        [FromQuery] List<string>? searchTags = null)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation(
                "Nhận yêu cầu lấy danh sách Todo với trang {PageNumber}, kích thước trang {PageSize}, isCompleted={searchName}, priority={searchDescription}",
                pageNumber, pageSize, searchName, searchDescription);

            // Kiểm tra và điều chỉnh thông số phân trang
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 1000000);
            
            // Tạo query
            var query = new GetProductsListQuery()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchName = searchName,
                searchDescription = searchDescription,
                searchTags = searchTags
            };

            // Gửi query đến handler
            var result = await _mediator.Send(query);

            // var productContext = result.Data.Items.Select(item => new Product()
            // {
            //     Id = Guid.Parse(item.Id),
            //     Name = item.Name,
            //     Description = item.Description,
            //     Category = item.Category,
            //     CreatedAt = item.CreatedAt,
            //     UpdatedAt = item.UpdatedAt,
            //     Dimensions = new Domain.Entities.Dimensions()
            //     {
            //         Height = item.Dimensions.Height,
            //         Length = item.Dimensions.Length,
            //         Width = item.Dimensions.Width
            //     },
            //     Manufacturer = new Domain.Entities.ManufacturerInfo()
            //     {
            //         Country = item.Manufacturer.Country,
            //         Name = item.Manufacturer.Name,
            //     },
            //     Material = item.Material,
            //     Price = item.Price,
            //     Status = item.Status,
            //     Tags = item.Tags,
            //     ManufacturingDate = item.ManufacturingDate
            // }).ToList();
            //
            // await _context.Products.AddRangeAsync(productContext);
            //
            // await _context.SaveChangesAsync();
            
            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã lấy thông tin Todo có ID: {ProductId} thành công", result.Data.ToString());
                return Ok(null);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi trong quá trình tạo data lên alasticsearch có vấn đề");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }
}
