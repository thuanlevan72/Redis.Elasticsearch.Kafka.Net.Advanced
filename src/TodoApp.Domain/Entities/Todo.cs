namespace TodoApp.Domain.Entities;

/// <summary>
/// Lớp đại diện cho một công việc cần làm
/// </summary>
public class Todo
{
    /// <summary>
    /// ID của công việc
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Tiêu đề của công việc
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về công việc
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Trạng thái hoàn thành của công việc
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Mức độ ưu tiên của công việc (thấp = 0, trung bình = 1, cao = 2)
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Ngày đến hạn của công việc
    /// </summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Ngày tạo công việc
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Ngày cập nhật công việc gần nhất
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Constructor mặc định cho Entity Framework
    /// </summary>
    private Todo() { }

    /// <summary>
    /// Khởi tạo một công việc mới
    /// </summary>
    /// <param name="title">Tiêu đề công việc</param>
    /// <param name="description">Mô tả công việc</param>
    /// <param name="priority">Mức độ ưu tiên</param>
    /// <param name="dueDate">Ngày đến hạn</param>
    public Todo(string title, string description, int priority = 0, DateTime? dueDate = null)
    {
        // Gán ID mới cho công việc
        Id = Guid.NewGuid();
        // Gán tiêu đề
        Title = title;
        // Gán mô tả
        Description = description;
        // Khởi tạo trạng thái là chưa hoàn thành
        IsCompleted = false;
        // Gán mức độ ưu tiên
        Priority = priority;
        // Gán ngày đến hạn
        DueDate = dueDate;
        // Gán ngày tạo là thời điểm hiện tại
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật thông tin công việc
    /// </summary>
    /// <param name="title">Tiêu đề mới</param>
    /// <param name="description">Mô tả mới</param>
    /// <param name="priority">Mức ưu tiên mới</param>
    /// <param name="dueDate">Ngày đến hạn mới</param>
    public void Update(string title, string description, int priority, DateTime? dueDate)
    {
        // Cập nhật tiêu đề
        Title = title;
        // Cập nhật mô tả
        Description = description;
        // Cập nhật mức ưu tiên
        Priority = priority;
        // Cập nhật ngày đến hạn
        DueDate = dueDate;
        // Cập nhật thời gian chỉnh sửa
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đánh dấu công việc là đã hoàn thành
    /// </summary>
    public void MarkAsCompleted()
    {
        // Nếu công việc đã hoàn thành rồi thì không làm gì
        if (IsCompleted) return;
        
        // Đánh dấu công việc là đã hoàn thành
        IsCompleted = true;
        // Cập nhật thời gian chỉnh sửa
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đánh dấu công việc là chưa hoàn thành
    /// </summary>
    public void MarkAsIncomplete()
    {
        // Nếu công việc chưa hoàn thành thì không làm gì
        if (!IsCompleted) return;
        
        // Đánh dấu công việc là chưa hoàn thành
        IsCompleted = false;
        // Cập nhật thời gian chỉnh sửa
        UpdatedAt = DateTime.UtcNow;
    }
}
