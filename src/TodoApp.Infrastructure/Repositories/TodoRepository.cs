using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Infrastructure.Data;

namespace TodoApp.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý các thao tác với entity Todo trong database
/// </summary>
public class TodoRepository : ITodoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TodoRepository> _logger;

    /// <summary>
    /// Khởi tạo repository với context và logger
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger</param>
    public TodoRepository(ApplicationDbContext context, ILogger<TodoRepository> logger)
    {
        // Lưu trữ context
        _context = context;
        // Lưu trữ logger
        _logger = logger;
    }

    /// <summary>
    /// Lấy tất cả các Todo
    /// </summary>
    /// <returns>Danh sách tất cả Todo</returns>
    public async Task<IEnumerable<Todo>> GetAllAsync()
    {
        try
        {
            // Truy vấn tất cả Todo, sắp xếp theo thời gian tạo giảm dần
            _logger.LogInformation("Lấy tất cả các Todo từ database");
            return await _context.Todos
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi lấy tất cả các Todo");
            throw;
        }
    }

    /// <summary>
    /// Lấy Todo theo ID
    /// </summary>
    /// <param name="id">ID của Todo cần lấy</param>
    /// <returns>Todo nếu tìm thấy, null nếu không tìm thấy</returns>
    public async Task<Todo?> GetByIdAsync(Guid id)
    {
        try
        {
            // Truy vấn Todo theo ID
            _logger.LogInformation("Lấy Todo có ID {TodoId} từ database", id);
            return await _context.Todos.FindAsync(id);
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi lấy Todo có ID {TodoId}", id);
            throw;
        }
    }

    /// <summary>
    /// Thêm Todo mới vào database
    /// </summary>
    /// <param name="entity">Todo cần thêm</param>
    public async Task AddAsync(Todo entity)
    {
        try
        {
            // Thêm Todo vào context
            _logger.LogInformation("Thêm Todo mới với ID {TodoId} vào database", entity.Id);
            await _context.Todos.AddAsync(entity);
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi thêm Todo mới với ID {TodoId}", entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Cập nhật Todo trong database
    /// </summary>
    /// <param name="entity">Todo cần cập nhật</param>
    public Task UpdateAsync(Todo entity)
    {
        try
        {
            // EntityFramework tự động theo dõi và cập nhật các thay đổi
            _logger.LogInformation("Cập nhật Todo có ID {TodoId} trong database", entity.Id);
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi cập nhật Todo có ID {TodoId}", entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Xóa Todo khỏi database
    /// </summary>
    /// <param name="entity">Todo cần xóa</param>
    public Task DeleteAsync(Todo entity)
    {
        try
        {
            // Xóa Todo khỏi context
            _logger.LogInformation("Xóa Todo có ID {TodoId} khỏi database", entity.Id);
            _context.Todos.Remove(entity);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi xóa Todo có ID {TodoId}", entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Lưu các thay đổi vào database
    /// </summary>
    public async Task SaveChangesAsync()
    {
        try
        {
            // Lưu tất cả các thay đổi vào database
            _logger.LogInformation("Lưu các thay đổi vào database");
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log lỗi cụ thể về database và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi cập nhật database khi lưu thay đổi: {Message}", ex.InnerException?.Message ?? ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Log lỗi không mong đợi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi không mong đợi khi lưu thay đổi vào database");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách Todo theo trạng thái hoàn thành
    /// </summary>
    /// <param name="isCompleted">Trạng thái hoàn thành</param>
    /// <returns>Danh sách Todo theo trạng thái</returns>
    public async Task<IEnumerable<Todo>> GetByCompletionStatusAsync(bool isCompleted)
    {
        try
        {
            // Truy vấn Todo theo trạng thái hoàn thành
            _logger.LogInformation("Lấy danh sách Todo theo trạng thái hoàn thành: {IsCompleted}", isCompleted);
            return await _context.Todos
                .Where(t => t.IsCompleted == isCompleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi lấy danh sách Todo theo trạng thái hoàn thành: {IsCompleted}", isCompleted);
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách Todo theo mức độ ưu tiên
    /// </summary>
    /// <param name="priority">Mức độ ưu tiên</param>
    /// <returns>Danh sách Todo theo mức độ ưu tiên</returns>
    public async Task<IEnumerable<Todo>> GetByPriorityAsync(int priority)
    {
        try
        {
            // Truy vấn Todo theo mức độ ưu tiên
            _logger.LogInformation("Lấy danh sách Todo theo mức độ ưu tiên: {Priority}", priority);
            return await _context.Todos
                .Where(t => t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi lấy danh sách Todo theo mức độ ưu tiên: {Priority}", priority);
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách Todo sắp đến hạn trong số ngày cụ thể
    /// </summary>
    /// <param name="days">Số ngày</param>
    /// <returns>Danh sách Todo sắp đến hạn</returns>
    public async Task<IEnumerable<Todo>> GetUpcomingAsync(int days)
    {
        try
        {
            // Tính toán ngày giới hạn
            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);
            
            // Truy vấn Todo sắp đến hạn
            _logger.LogInformation("Lấy danh sách Todo sắp đến hạn trong {Days} ngày tới", days);
            return await _context.Todos
                .Where(t => t.DueDate.HasValue && 
                            t.DueDate.Value.Date >= today && 
                            t.DueDate.Value.Date <= futureDate &&
                            !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi lấy danh sách Todo sắp đến hạn trong {Days} ngày tới", days);
            throw;
        }
    }
}
