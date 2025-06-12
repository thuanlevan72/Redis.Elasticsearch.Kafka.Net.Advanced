using Microsoft.AspNetCore.Mvc;
using MediatR;
using StackExchange.Redis;
using TodoApp.Application.Todos.Commands.CreateTodo;
using TodoApp.Application.Todos.Commands.UpdateTodo;
using TodoApp.Application.Todos.Commands.DeleteTodo;

namespace TodoApp.WriteApi.Controllers;

/// <summary>
/// API controller cho thao tác ghi với Todo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TodosController> _logger;
    private readonly IConnectionMultiplexer _redis;

    /// <summary>
    /// Khởi tạo controller với mediator và logger
    /// </summary>
    /// <param name="mediator">Mediator để gửi command</param>
    /// <param name="logger">Logger</param>
    public TodosController(IMediator mediator, ILogger<TodosController> logger, IConnectionMultiplexer redis)
    {
        // Lưu trữ mediator
        _mediator = mediator;
        // Lưu trữ logger
        _logger = logger;
        _redis = redis;
    }

    /// <summary>
    /// Tạo một Todo mới
    /// </summary>
    /// <param name="command">Thông tin Todo cần tạo</param>
    /// <returns>Kết quả tạo Todo</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTodoCommand command)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu tạo Todo mới với tiêu đề: {Title}", command.Title);

            // Gửi command đến handler
            var result = await _mediator.Send(command);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công với ID của Todo
                _logger.LogInformation("Đã tạo Todo mới thành công với ID: {TodoId}", result.Data);
                return CreatedAtAction(nameof(Create), new { id = result.Data }, result.Data);
            }

            // Trả về lỗi nếu có
            _logger.LogWarning("Không thể tạo Todo mới: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi tạo Todo mới");
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }

    /// <summary>
    /// Cập nhật một Todo
    /// </summary>
    /// <param name="id">ID của Todo cần cập nhật</param>
    /// <param name="command">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật Todo</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoCommand command)
    {
        try
        {
            // Kiểm tra ID trong route và body
            if (id != command.Id)
            {
                _logger.LogWarning("ID trong route ({RouteId}) không khớp với ID trong body ({BodyId})", id, command.Id);
                return BadRequest("ID trong route không khớp với ID trong body");
            }

            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu cập nhật Todo có ID: {TodoId}", id);

            // Gửi command đến handler
            var result = await _mediator.Send(command);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã cập nhật Todo có ID: {TodoId} thành công", id);
                return NoContent();
            }

            // Kiểm tra lỗi không tìm thấy
            if (result.Errors.Any(e => e.Contains("Không tìm thấy")))
            {
                _logger.LogWarning("Không tìm thấy Todo có ID: {TodoId}", id);
                return NotFound(result.Errors);
            }

            // Trả về lỗi khác nếu có
            _logger.LogWarning("Không thể cập nhật Todo có ID: {TodoId}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi cập nhật Todo có ID: {TodoId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }

    /// <summary>
    /// Xóa một Todo
    /// </summary>
    /// <param name="id">ID của Todo cần xóa</param>
    /// <returns>Kết quả xóa Todo</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu xóa Todo có ID: {TodoId}", id);

            // Tạo command xóa
            var command = new DeleteTodoCommand { Id = id };

            // Gửi command đến handler
            var result = await _mediator.Send(command);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã xóa Todo có ID: {TodoId} thành công", id);
                return NoContent();
            }

            // Kiểm tra lỗi không tìm thấy
            if (result.Errors.Any(e => e.Contains("Không tìm thấy")))
            {
                _logger.LogWarning("Không tìm thấy Todo có ID: {TodoId}", id);
                return NotFound(result.Errors);
            }

            // Trả về lỗi khác nếu có
            _logger.LogWarning("Không thể xóa Todo có ID: {TodoId}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi xóa Todo có ID: {TodoId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }

    /// <summary>
    /// Đánh dấu một Todo là đã hoàn thành
    /// </summary>
    /// <param name="id">ID của Todo cần đánh dấu</param>
    /// <returns>Kết quả cập nhật Todo</returns>
    [HttpPatch("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsCompleted(Guid id)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu đánh dấu Todo có ID: {TodoId} là đã hoàn thành", id);

            // Lấy thông tin Todo hiện tại (trong môi trường thực tế, nên sử dụng query riêng)
            // Tạm thời sử dụng cách đơn giản: tạo command cập nhật với IsCompleted = true
            var command = new UpdateTodoCommand 
            { 
                Id = id, 
                IsCompleted = true,
                // Các trường khác sẽ được cập nhật trong handler dựa trên dữ liệu hiện có
                Title = "Placeholder", // Sẽ được ghi đè trong handler
                Description = "Placeholder", // Sẽ được ghi đè trong handler
                Priority = 0, // Sẽ được ghi đè trong handler
                DueDate = null // Sẽ được ghi đè trong handler
            };

            // Gửi command đến handler
            var result = await _mediator.Send(command);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã đánh dấu Todo có ID: {TodoId} là đã hoàn thành", id);
                return NoContent();
            }

            // Kiểm tra lỗi không tìm thấy
            if (result.Errors.Any(e => e.Contains("Không tìm thấy")))
            {
                _logger.LogWarning("Không tìm thấy Todo có ID: {TodoId}", id);
                return NotFound(result.Errors);
            }

            // Trả về lỗi khác nếu có
            _logger.LogWarning("Không thể đánh dấu Todo có ID: {TodoId} là đã hoàn thành: {Errors}", 
                id, string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi đánh dấu Todo có ID: {TodoId} là đã hoàn thành", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }

    /// <summary>
    /// Đánh dấu một Todo là chưa hoàn thành
    /// </summary>
    /// <param name="id">ID của Todo cần đánh dấu</param>
    /// <returns>Kết quả cập nhật Todo</returns>
    [HttpPatch("{id}/incomplete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsIncomplete(Guid id)
    {
        try
        {
            // Log thông tin yêu cầu
            _logger.LogInformation("Nhận yêu cầu đánh dấu Todo có ID: {TodoId} là chưa hoàn thành", id);

            // Tương tự như MarkAsCompleted, nhưng với IsCompleted = false
            var command = new UpdateTodoCommand 
            { 
                Id = id, 
                IsCompleted = false,
                // Các trường khác sẽ được cập nhật trong handler dựa trên dữ liệu hiện có
                Title = "Placeholder", // Sẽ được ghi đè trong handler
                Description = "Placeholder", // Sẽ được ghi đè trong handler
                Priority = 0, // Sẽ được ghi đè trong handler
                DueDate = null // Sẽ được ghi đè trong handler
            };

            // Gửi command đến handler
            var result = await _mediator.Send(command);

            // Kiểm tra kết quả
            if (result.Succeeded)
            {
                // Trả về kết quả thành công
                _logger.LogInformation("Đã đánh dấu Todo có ID: {TodoId} là chưa hoàn thành", id);
                return NoContent();
            }

            // Kiểm tra lỗi không tìm thấy
            if (result.Errors.Any(e => e.Contains("Không tìm thấy")))
            {
                _logger.LogWarning("Không tìm thấy Todo có ID: {TodoId}", id);
                return NotFound(result.Errors);
            }

            // Trả về lỗi khác nếu có
            _logger.LogWarning("Không thể đánh dấu Todo có ID: {TodoId} là chưa hoàn thành: {Errors}", 
                id, string.Join(", ", result.Errors));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            // Log lỗi và trả về lỗi máy chủ
            _logger.LogError(ex, "Lỗi không mong đợi khi đánh dấu Todo có ID: {TodoId} là chưa hoàn thành", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi máy chủ khi xử lý yêu cầu");
        }
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromQuery] string message)
    {
        var pub = _redis.GetSubscriber();
        await pub.PublishAsync("chat-room", message);
        return Ok("📨 Sent");
    }
}
