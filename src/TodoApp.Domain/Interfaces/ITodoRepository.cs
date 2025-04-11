using TodoApp.Domain.Entities;

namespace TodoApp.Domain.Interfaces;

/// <summary>
/// Giao diện repository cho entity Todo
/// </summary>
public interface ITodoRepository : IRepository<Todo>
{
    /// <summary>
    /// Lấy danh sách todos theo trạng thái hoàn thành
    /// </summary>
    /// <param name="isCompleted">Trạng thái hoàn thành</param>
    /// <returns>Danh sách todos theo trạng thái</returns>
    Task<IEnumerable<Todo>> GetByCompletionStatusAsync(bool isCompleted);

    /// <summary>
    /// Lấy danh sách todos theo mức độ ưu tiên
    /// </summary>
    /// <param name="priority">Mức độ ưu tiên</param>
    /// <returns>Danh sách todos theo mức độ ưu tiên</returns>
    Task<IEnumerable<Todo>> GetByPriorityAsync(int priority);

    /// <summary>
    /// Lấy danh sách todos sắp đến hạn trong khoảng thời gian
    /// </summary>
    /// <param name="days">Số ngày</param>
    /// <returns>Danh sách todos sắp đến hạn</returns>
    Task<IEnumerable<Todo>> GetUpcomingAsync(int days);
}
