namespace TodoApp.Domain.Interfaces;

/// <summary>
/// Giao diện cơ sở cho tất cả các Repository
/// </summary>
/// <typeparam name="T">Kiểu entity</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Lấy tất cả các entity
    /// </summary>
    /// <returns>Danh sách entity</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Lấy entity theo ID
    /// </summary>
    /// <param name="id">ID của entity cần lấy</param>
    /// <returns>Entity nếu tìm thấy, null nếu không tìm thấy</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Thêm entity mới
    /// </summary>
    /// <param name="entity">Entity cần thêm</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Cập nhật entity
    /// </summary>
    /// <param name="entity">Entity cần cập nhật</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Xóa entity
    /// </summary>
    /// <param name="entity">Entity cần xóa</param>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Lưu các thay đổi vào database
    /// </summary>
    Task SaveChangesAsync();
}
