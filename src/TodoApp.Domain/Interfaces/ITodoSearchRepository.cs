using TodoApp.Domain.Entities;

namespace TodoApp.Domain.Interfaces;

/// <summary>
/// Giao diện repository tìm kiếm cho entity Todo
/// </summary>
public interface ITodoSearchRepository
{
    /// <summary>
    /// Tìm kiếm todos theo từ khóa
    /// </summary>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="page">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách todos phù hợp với từ khóa</returns>
    Task<(IEnumerable<Todo> Items, long TotalCount)> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);

    /// <summary>
    /// Đồng bộ một todo vào Elasticsearch
    /// </summary>
    /// <param name="todo">Todo cần đồng bộ</param>
    Task IndexAsync(Todo todo);

    /// <summary>
    /// Xóa một todo khỏi Elasticsearch
    /// </summary>
    /// <param name="todoId">ID của todo cần xóa</param>
    Task DeleteAsync(Guid todoId);
}
