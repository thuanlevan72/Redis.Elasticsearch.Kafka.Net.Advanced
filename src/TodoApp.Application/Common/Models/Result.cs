namespace TodoApp.Application.Common.Models;

/// <summary>
/// Lớp kết quả chung cho các thao tác
/// </summary>
public class Result
{
    /// <summary>
    /// Trạng thái thành công
    /// </summary>
    public bool Succeeded { get; }
    
    /// <summary>
    /// Danh sách lỗi nếu có
    /// </summary>
    public string[] Errors { get; }

    /// <summary>
    /// Khởi tạo kết quả
    /// </summary>
    /// <param name="succeeded">Trạng thái thành công</param>
    /// <param name="errors">Danh sách lỗi</param>
    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        // Gán trạng thái thành công
        Succeeded = succeeded;
        // Gán danh sách lỗi
        Errors = errors.ToArray();
    }

    /// <summary>
    /// Tạo kết quả thành công
    /// </summary>
    /// <returns>Kết quả thành công</returns>
    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    /// <summary>
    /// Tạo kết quả thất bại với danh sách lỗi
    /// </summary>
    /// <param name="errors">Danh sách lỗi</param>
    /// <returns>Kết quả thất bại</returns>
    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}

/// <summary>
/// Lớp kết quả có dữ liệu trả về
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Dữ liệu trả về
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Khởi tạo kết quả có dữ liệu
    /// </summary>
    /// <param name="succeeded">Trạng thái thành công</param>
    /// <param name="data">Dữ liệu trả về</param>
    /// <param name="errors">Danh sách lỗi</param>
    protected Result(bool succeeded, T data, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        // Gán dữ liệu trả về
        Data = data;
    }

    /// <summary>
    /// Tạo kết quả thành công với dữ liệu
    /// </summary>
    /// <param name="data">Dữ liệu trả về</param>
    /// <returns>Kết quả thành công với dữ liệu</returns>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, Array.Empty<string>());
    }

    /// <summary>
    /// Tạo kết quả thất bại với danh sách lỗi
    /// </summary>
    /// <param name="errors">Danh sách lỗi</param>
    /// <returns>Kết quả thất bại</returns>
    public new static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default!, errors);
    }
}
