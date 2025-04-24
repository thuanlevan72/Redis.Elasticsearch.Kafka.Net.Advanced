/// Lớp ProductDocument đại diện cho một tài liệu sản phẩm trong Elasticsearch
using System.Text.Json;
using System.Text.Json.Serialization;
namespace TodoApp.Application.Common.Models;
public class ProductDocument
{
    // Định danh duy nhất, lưu dưới dạng chuỗi từ Guid để tương thích với Elasticsearch
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Tên sản phẩm, ánh xạ là text để hỗ trợ tìm kiếm full-text
    public string Name { get; set; } = string.Empty;

    // Mô tả sản phẩm, ánh xạ là text để hỗ trợ tìm kiếm full-text
    public string Description { get; set; } = string.Empty;

    // Giá sản phẩm, ánh xạ là double để hỗ trợ truy vấn khoảng
    public double Price { get; set; }

    // Loại sản phẩm, ánh xạ là keyword để tìm kiếm chính xác
    public string Category { get; set; } = string.Empty;

    // Vật liệu sản phẩm, ánh xạ là keyword để tìm kiếm chính xác
    public string Material { get; set; } = string.Empty;

    // Ngày sản xuất, ánh xạ là date để hỗ trợ truy vấn thời gian
    public DateTime ManufacturingDate { get; set; }

    // Trạng thái sản phẩm, ánh xạ là keyword để tìm kiếm chính xác
    public string Status { get; set; } = string.Empty;

    // Kích thước sản phẩm, ánh xạ là object để truy vấn các trường con
    public Dimensions Dimensions { get; set; } = new Dimensions();

    // Danh sách thẻ, ánh xạ là keyword để tìm kiếm chính xác
    public List<string> Tags { get; set; } = new List<string>();

    // Thông tin nhà sản xuất, ánh xạ là object để truy vấn các trường con
    public ManufacturerInfo Manufacturer { get; set; } = new ManufacturerInfo();
    
    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Ngày cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

// Lớp Dimensions lưu kích thước sản phẩm
public class Dimensions
{
    // Chiều dài (mm), ánh xạ là double
    public double Length { get; set; }

    // Chiều rộng (mm), ánh xạ là double
    public double Width { get; set; }

    // Chiều cao (mm), ánh xạ là double
    public double Height { get; set; }
}

// Lớp ManufacturerInfo lưu thông tin nhà sản xuất
public class ManufacturerInfo
{
    // Tên nhà sản xuất, ánh xạ là text để hỗ trợ tìm kiếm
    public string Name { get; set; } = string.Empty;
    // Quốc gia, ánh xạ là keyword để tìm kiếm chính xác
    public string Country { get; set; } = string.Empty;
}