namespace TodoApp.Application.Common.Models;

/// <summary>
/// Mô hình Todo cho Elasticsearch
/// </summary>
public class TodoDocument
{
    /// <summary>
    /// ID của Todo
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tiêu đề của Todo
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả của Todo
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Trạng thái hoàn thành
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Mức độ ưu tiên
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Ngày hạn
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Ngày cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}