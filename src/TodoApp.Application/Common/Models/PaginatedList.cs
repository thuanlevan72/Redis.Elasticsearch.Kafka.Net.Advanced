namespace TodoApp.Application.Common.Models;

/// <summary>
/// Lớp đại diện cho danh sách phân trang
/// </summary>
/// <typeparam name="T">Kiểu của item trong danh sách</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Danh sách các item
    /// </summary>
    public IReadOnlyCollection<T> Items { get; }
    
    /// <summary>
    /// Trang hiện tại
    /// </summary>
    public int PageNumber { get; }
    
    /// <summary>
    /// Tổng số trang
    /// </summary>
    public int TotalPages { get; }
    
    /// <summary>
    /// Tổng số item
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    /// Khởi tạo danh sách phân trang
    /// </summary>
    /// <param name="items">Danh sách các item</param>
    /// <param name="totalCount">Tổng số item</param>
    /// <param name="pageNumber">Trang hiện tại</param>
    /// <param name="pageSize">Kích thước trang</param>
    public PaginatedList(IEnumerable<T> items, long totalCount, int pageNumber, int pageSize)
    {
        // Tính tổng số trang dựa trên tổng số item và kích thước trang
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        // Lưu trữ các thông tin
        PageNumber = pageNumber;
        TotalCount = totalCount;
        Items = items.ToList().AsReadOnly();
    }

    /// <summary>
    /// Kiểm tra xem có trang trước không
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Kiểm tra xem có trang sau không
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Tạo danh sách phân trang từ nguồn dữ liệu
    /// </summary>
    /// <param name="source">Nguồn dữ liệu</param>
    /// <param name="pageNumber">Trang hiện tại</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách phân trang</returns>
    public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        // Đếm tổng số item
        var count = source.Count();
        
        // Lấy các item cho trang hiện tại
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        // Tạo và trả về danh sách phân trang
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Tạo danh sách phân trang từ nguồn dữ liệu và tổng số item
    /// </summary>
    /// <param name="items">Các item cho trang hiện tại</param>
    /// <param name="totalCount">Tổng số item</param>
    /// <param name="pageNumber">Trang hiện tại</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách phân trang</returns>
    public static PaginatedList<T> Create(IEnumerable<T> items, long totalCount, int pageNumber, int pageSize)
    {
        // Tạo và trả về danh sách phân trang
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
