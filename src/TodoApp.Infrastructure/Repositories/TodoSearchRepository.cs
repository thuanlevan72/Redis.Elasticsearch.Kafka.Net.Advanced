using Microsoft.Extensions.Logging;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Models;

namespace TodoApp.Infrastructure.Repositories;

/// <summary>
/// Repository xử lý tìm kiếm Todo trong Elasticsearch
/// </summary>
public class TodoSearchRepository : ITodoSearchRepository
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<TodoSearchRepository> _logger;
    private const string IndexName = "todos";

    /// <summary>
    /// Khởi tạo repository với dịch vụ Elasticsearch và logger
    /// </summary>
    /// <param name="elasticsearchService">Dịch vụ Elasticsearch</param>
    /// <param name="logger">Logger</param>
    public TodoSearchRepository(
        IElasticsearchService elasticsearchService,
        ILogger<TodoSearchRepository> logger)
    {
        // Lưu trữ dịch vụ Elasticsearch
        _elasticsearchService = elasticsearchService;
        // Lưu trữ logger
        _logger = logger;
    }

    /// <summary>
    /// Tìm kiếm todos theo từ khóa
    /// </summary>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="page">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách todos phù hợp với từ khóa và tổng số kết quả</returns>
    public async Task<(IEnumerable<Todo> Items, long TotalCount)> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Tìm kiếm Todo với từ khóa '{SearchTerm}', trang {Page}, kích thước trang {PageSize}", 
                searchTerm, page, pageSize);

            // Tìm kiếm TodoDocument trong Elasticsearch
            var (documents, totalCount) = await _elasticsearchService.SearchAsync<TodoDocument>(
                IndexName, 
                searchTerm, 
                page, 
                pageSize);

            // Chuyển đổi TodoDocument thành Todo
            var todos = documents.Select(MapToTodo).ToList();

            _logger.LogInformation("Đã tìm thấy {Count} Todo trong tổng số {TotalCount} kết quả", 
                todos.Count, totalCount);

            // Trả về kết quả
            return (todos, totalCount);
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi tìm kiếm Todo với từ khóa '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Đồng bộ một todo vào Elasticsearch
    /// </summary>
    /// <param name="todo">Todo cần đồng bộ</param>
    public async Task IndexAsync(Todo todo)
    {
        try
        {
            _logger.LogInformation("Đồng bộ Todo có ID {TodoId} vào Elasticsearch", todo.Id);

            // Chuyển đổi Todo thành TodoDocument
            var document = MapToDocument(todo);

            // Lưu trữ vào Elasticsearch
            var result = await _elasticsearchService.IndexDocumentAsync(
                IndexName, 
                document, 
                todo.Id.ToString());

            // Log kết quả
            if (result)
            {
                _logger.LogInformation("Đã đồng bộ Todo có ID {TodoId} vào Elasticsearch thành công", todo.Id);
            }
            else
            {
                _logger.LogWarning("Không thể đồng bộ Todo có ID {TodoId} vào Elasticsearch", todo.Id);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi đồng bộ Todo có ID {TodoId} vào Elasticsearch", todo.Id);
            throw;
        }
    }

    /// <summary>
    /// Xóa một todo khỏi Elasticsearch
    /// </summary>
    /// <param name="todoId">ID của todo cần xóa</param>
    public async Task DeleteAsync(Guid todoId)
    {
        try
        {
            _logger.LogInformation("Xóa Todo có ID {TodoId} khỏi Elasticsearch", todoId);

            // Xóa khỏi Elasticsearch
            var result = await _elasticsearchService.DeleteDocumentAsync(
                IndexName, 
                todoId.ToString());

            // Log kết quả
            if (result)
            {
                _logger.LogInformation("Đã xóa Todo có ID {TodoId} khỏi Elasticsearch thành công", todoId);
            }
            else
            {
                _logger.LogWarning("Không thể xóa Todo có ID {TodoId} khỏi Elasticsearch", todoId);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi xóa Todo có ID {TodoId} khỏi Elasticsearch", todoId);
            throw;
        }
    }

    /// <summary>
    /// Chuyển đổi từ TodoDocument sang Todo
    /// </summary>
    /// <param name="document">TodoDocument cần chuyển đổi</param>
    /// <returns>Todo sau khi chuyển đổi</returns>
    private Todo MapToTodo(TodoDocument document)
    {
        try
        {
            // Tạo Todo từ dữ liệu trong document
            var todo = new Todo(
                document.Title,
                document.Description ?? string.Empty,
                document.Priority,
                document.DueDate
            );

            // Phản chiếu các thuộc tính không thể thiết lập qua constructor
            var todoType = typeof(Todo);
            
            // Thiết lập ID
            var idProperty = todoType.GetProperty("Id");
            idProperty?.SetValue(todo, document.Id);
            
            // Thiết lập trạng thái hoàn thành
            var isCompletedProperty = todoType.GetProperty("IsCompleted");
            isCompletedProperty?.SetValue(todo, document.IsCompleted);
            
            // Thiết lập ngày tạo
            var createdAtProperty = todoType.GetProperty("CreatedAt");
            createdAtProperty?.SetValue(todo, document.CreatedAt);
            
            // Thiết lập ngày cập nhật
            var updatedAtProperty = todoType.GetProperty("UpdatedAt");
            updatedAtProperty?.SetValue(todo, document.UpdatedAt);

            return todo;
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi chuyển đổi TodoDocument sang Todo");
            throw;
        }
    }

    /// <summary>
    /// Chuyển đổi từ Todo sang TodoDocument
    /// </summary>
    /// <param name="todo">Todo cần chuyển đổi</param>
    /// <returns>TodoDocument sau khi chuyển đổi</returns>
    private TodoDocument MapToDocument(Todo todo)
    {
        try
        {
            // Tạo TodoDocument từ dữ liệu trong Todo
            return new TodoDocument
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                Priority = todo.Priority,
                DueDate = todo.DueDate,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            // Log lỗi và ném lại ngoại lệ
            _logger.LogError(ex, "Lỗi khi chuyển đổi Todo sang TodoDocument");
            throw;
        }
    }
}
