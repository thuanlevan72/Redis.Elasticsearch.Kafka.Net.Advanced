namespace TodoApp.Domain.Exceptions;

/// <summary>
/// Lớp ngoại lệ cơ sở cho tất cả các ngoại lệ trong lớp domain
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Khởi tạo một ngoại lệ domain với thông báo lỗi
    /// </summary>
    /// <param name="message">Thông báo lỗi</param>
    protected DomainException(string message) : base(message)
    {
    }
}

/// <summary>
/// Ngoại lệ khi không tìm thấy Todo
/// </summary>
public class TodoNotFoundException : DomainException
{
    /// <summary>
    /// Khởi tạo ngoại lệ khi không tìm thấy Todo theo ID
    /// </summary>
    /// <param name="todoId">ID của Todo không tìm thấy</param>
    public TodoNotFoundException(Guid todoId) 
        : base($"Không tìm thấy công việc có ID '{todoId}'")
    {
    }
}

/// <summary>
/// Ngoại lệ khi dữ liệu Todo không hợp lệ
/// </summary>
public class TodoInvalidException : DomainException
{
    /// <summary>
    /// Khởi tạo ngoại lệ khi dữ liệu Todo không hợp lệ
    /// </summary>
    /// <param name="message">Thông báo lỗi cụ thể</param>
    public TodoInvalidException(string message) : base(message)
    {
    }
}
