namespace TodoApp.Domain.Entities;
// Lớp Product đại diện cho một sản phẩm nha khoa trong hệ thống
public class Product
{
    // Định danh duy nhất của sản phẩm, sử dụng Guid để đảm bảo tính độc nhất
    public Guid Id { get; set; }

    // Tên sản phẩm, ví dụ: "Monolithic Denture", "Surgical Guide"
    public string Name { get; set; } = string.Empty;

    // Mô tả chi tiết về sản phẩm, hỗ trợ tìm kiếm full-text
    public string Description { get; set; } = string.Empty;

    // Giá sản phẩm (đơn vị tiền tệ, ví dụ: USD), hỗ trợ truy vấn khoảng
    public decimal Price { get; set; }

    // Loại sản phẩm, ví dụ: "Denture", "Crown", "Guide", dùng để phân loại
    public string Category { get; set; } = string.Empty;

    // Vật liệu chế tạo sản phẩm, ví dụ: "Zirconia", "Acrylic", "Titanium"
    public string Material { get; set; } = string.Empty;

    // Ngày sản xuất sản phẩm, hỗ trợ truy vấn theo thời gian
    public DateTime ManufacturingDate { get; set; }

    // Trạng thái sản phẩm, ví dụ: "InStock", "OutOfStock", "Discontinued"
    public string Status { get; set; } = string.Empty;

    // Kích thước sản phẩm (dài, rộng, cao), lưu dưới dạng đối tượng lồng
    public Dimensions Dimensions { get; set; } = new Dimensions();

    // Danh sách các thẻ mô tả đặc tính sản phẩm, ví dụ: "Antibacterial", "HighPrecision"
    public List<string> Tags { get; set; } = new List<string>();

    // Thông tin nhà sản xuất, lưu dưới dạng đối tượng lồng
    public ManufacturerInfo Manufacturer { get; set; } = new ManufacturerInfo();
    
    
}

// Lớp Dimensions lưu thông tin kích thước của sản phẩm (đơn vị: mm)
public class Dimensions
{
    // Chiều dài của sản phẩm (mm)
    public double Length { get; set; }

    // Chiều rộng của sản phẩm (mm)
    public double Width { get; set; }

    // Chiều cao của sản phẩm (mm)
    public double Height { get; set; }
}

// Lớp ManufacturerInfo lưu thông tin về nhà sản xuất của sản phẩm
public class ManufacturerInfo
{
    // Tên nhà sản xuất, ví dụ: "AvaDent", "DentalTech"
    public string Name { get; set; } = string.Empty;

    // Quốc gia của nhà sản xuất, ví dụ: "USA", "Germany"
    public string Country { get; set; } = string.Empty;
}