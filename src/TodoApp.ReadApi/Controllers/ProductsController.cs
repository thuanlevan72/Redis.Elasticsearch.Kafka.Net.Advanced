using Microsoft.AspNetCore.Mvc;
using MediatR;
using TodoApp.Application.Todos.Commands;
using TodoApp.Application.Todos.Queries.GetTodoById;
using TodoApp.Application.Todos.Queries.GetTodosList;
using TodoApp.Application.Todos.Queries.SearchTodos;

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

    /// <summary>
    /// Khởi tạo controller với mediator và logger
    /// </summary>
    /// <param name="mediator">Mediator để gửi query</param>
    /// <param name="logger">Logger</param>
    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        // Lưu trữ mediator
        _mediator = mediator;
        // Lưu trữ logger
        _logger = logger;
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
}
