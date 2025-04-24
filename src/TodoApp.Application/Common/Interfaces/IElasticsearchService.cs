namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Interface cho dịch vụ Elasticsearch
/// </summary>
public interface IElasticsearchService
{
    /// <summary>
    /// Kiểm tra xem một chỉ mục có tồn tại không
    /// </summary>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <returns>True nếu chỉ mục tồn tại, ngược lại False</returns>
    Task<bool> IndexExistsAsync(string indexName);

    /// <summary>
    /// Tạo một chỉ mục mới
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <returns>True nếu tạo thành công, ngược lại False</returns>
    Task<bool> CreateIndexAsync<T>(string indexName) where T : class;

    /// <summary>
    /// Lưu trữ một tài liệu vào chỉ mục
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="document">Tài liệu cần lưu trữ</param>
    /// <param name="id">ID của tài liệu</param>
    /// <returns>True nếu lưu trữ thành công, ngược lại False</returns>
    Task<bool> IndexDocumentAsync<T>(string indexName, T document, string id) where T : class;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="documents"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> IndexAllDocumentAsync<T>(string indexName, List<T> documents) where T : class;

    /// <summary>
    /// Lấy một tài liệu từ chỉ mục theo ID
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="id">ID của tài liệu cần lấy</param>
    /// <returns>Tài liệu nếu tìm thấy, ngược lại null</returns>
    Task<T?> GetDocumentAsync<T>(string indexName, string id) where T : class;

    /// <summary>
    /// Xóa một tài liệu khỏi chỉ mục
    /// </summary>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="id">ID của tài liệu cần xóa</param>
    /// <returns>True nếu xóa thành công, ngược lại False</returns>
    Task<bool> DeleteDocumentAsync(string indexName, string id);

    /// <summary>
    /// Tìm kiếm tài liệu theo từ khóa
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu tài liệu</typeparam>
    /// <param name="indexName">Tên chỉ mục</param>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="page">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách tài liệu phù hợp và tổng số kết quả</returns>
    Task<(IEnumerable<T> Items, long TotalCount)> SearchAsync<T>(string indexName, string searchTerm, int page = 1, int pageSize = 10) where T : class;
}