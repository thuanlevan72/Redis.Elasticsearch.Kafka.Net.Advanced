using Microsoft.AspNetCore.Mvc;
using MediatR;
using TodoApp.Application.Todos.Queries.GetTodoById;
using TodoApp.Application.Todos.Queries.GetTodosList;
using TodoApp.Application.Todos.Queries.SearchTodos;

namespace TodoApp.ReadApi.Controllers;

/// <summary>
/// API controller cho thao tác đọc với Todo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TodosController> _logger;

    /// <summary>
    /// Khởi tạo controller với mediator và logger
    /// </summary>
    /// <param name="mediator">Mediator để gửi query</param>
    /// <param name="logger">Logger</param>
    public TodosController(IMediator mediator, ILogger<TodosController> logger)
    {
        // Lưu trữ mediator
        _mediator = mediator;
        // Lưu trữ logger
        _logger = logger;
    }

    /// <summary>
    /// Lấy thông tin Todo theo ID
    /// </summary>
    /// <param name="id">ID của Todo cần lấy</param>
    /// <returns>Thông tin chi tiết Todo</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu lấy thông tin Todo có ID: {TodoId}", id);

            // Tạo query
            var query = new GetTodoByIdQuery { Id = id };

            // Gửi query đến handler
            var result = await _mediator.Send(query);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã lấy thông tin Todo có ID: {TodoId} thành công", id);
                return Ok(result.Data);
            }

            // Kiểm tra lỗi không tìm thấy
            if (result.Errors.Any(e => e.Contains("Không tìm thấy")))
            {
                _logger.LogWarning("Không tìm thấy Todo có ID: {TodoId}", id);
                return NotFound(result.Errors);
            }

            // Trả về lỗi khác nếu có
            _logger.LogWarning("Không thể lấy thông tin Todo có ID: {TodoId}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi lấy thông tin Todo có ID: {TodoId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }

    /// <summary>
    /// Lấy danh sách Todo với phân trang và bộ lọc
    /// </summary>
    /// <param name="pageNumber">Số trang (mặc định là 1)</param>
    /// <param name="pageSize">Kích thước trang (mặc định là 10)</param>
    /// <param name="isCompleted">Lọc theo trạng thái hoàn thành</param>
    /// <param name="priority">Lọc theo mức độ ưu tiên</param>
    /// <returns>Danh sách Todo phân trang</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] int? priority = null)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation(
                "Nhận yêu cầu lấy danh sách Todo với trang {PageNumber}, kích thước trang {PageSize}, isCompleted={IsCompleted}, priority={Priority}",
                pageNumber, pageSize, isCompleted, priority);

            // Kiểm tra và điều chỉnh thông số phân trang
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            // Tạo query
            var query = new GetTodosListQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsCompleted = isCompleted,
                Priority = priority
            };

            // Gửi query đến handler
            var result = await _mediator.Send(query);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation(
                    "Đã lấy danh sách Todo thành công: {Count} items trên tổng số {TotalCount}",
                    result.Data.Items.Count, result.Data.TotalCount);
                
                // Trả về đối tượng kết quả đã được định dạng
                return Ok(new
                {
                    items = result.Data.Items,
                    pageNumber = result.Data.PageNumber,
                    totalPages = result.Data.TotalPages,
                    totalCount = result.Data.TotalCount,
                    hasPreviousPage = result.Data.HasPreviousPage,
                    hasNextPage = result.Data.HasNextPage
                });
            }

            // Trả về lỗi nếu có
            _logger.LogWarning("Không thể lấy danh sách Todo: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi lấy danh sách Todo");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }

    /// <summary>
    /// Tìm kiếm Todo theo từ khóa
    /// </summary>
    /// <param name="term">Từ khóa tìm kiếm</param>
    /// <param name="pageNumber">Số trang (mặc định là 1)</param>
    /// <param name="pageSize">Kích thước trang (mặc định là 10)</param>
    /// <returns>Kết quả tìm kiếm Todo phân trang</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string term = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation(
                "Nhận yêu cầu tìm kiếm Todo với từ khóa '{Term}', trang {PageNumber}, kích thước trang {PageSize}",
                term, pageNumber, pageSize);

            // Kiểm tra và điều chỉnh thông số phân trang
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            // Tạo query
            var query = new SearchTodosQuery
            {
                SearchTerm = term,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Gửi query đến handler
            var result = await _mediator.Send(query);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation(
                    "Đã tìm kiếm Todo thành công với từ khóa '{Term}': {Count} items trên tổng số {TotalCount}",
                    term, result.Data.Items.Count, result.Data.TotalCount);
                
                // Trả về đối tượng kết quả đã được định dạng
                return Ok(new
                {
                    items = result.Data.Items,
                    pageNumber = result.Data.PageNumber,
                    totalPages = result.Data.TotalPages,
                    totalCount = result.Data.TotalCount,
                    hasPreviousPage = result.Data.HasPreviousPage,
                    hasNextPage = result.Data.HasNextPage
                });
            }

            // Trả về lỗi nếu có
            _logger.LogWarning("Không thể tìm kiếm Todo với từ khóa '{Term}': {Errors}", 
                term, string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi tìm kiếm Todo với từ khóa '{Term}'", term);
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }
}
