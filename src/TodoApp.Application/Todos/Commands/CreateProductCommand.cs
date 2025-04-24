using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Bogus;
using MediatR;
using Microsoft.Extensions.Logging;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Models;
using TodoApp.Application.Common.Interfaces;
using Dimensions = TodoApp.Application.Common.Models.Dimensions;
using ManufacturerInfo = TodoApp.Application.Common.Models.ManufacturerInfo;

namespace TodoApp.Application.Todos.Commands;

/// <summary>
/// DTO cho command tạo Product mới
/// </summary>
public class CreateProductCommand : IRequest<Result<List<Guid>>>
{
    /// <summary>
    /// Tiêu đề của Lệnh tạo Product
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    public int QuantityAdd { get; set; }
}


public class ProductDocumentEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("material")]
    public string Material { get; set; }

    [JsonPropertyName("manufacturing_date")]
    public long ManufacturingDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("dimensions_length")]
    public double DimensionsLength { get; set; }

    [JsonPropertyName("dimensions_width")]
    public double DimensionsWidth { get; set; }

    [JsonPropertyName("dimensions_height")]
    public double DimensionsHeight { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("manufacturer_name")]
    public string ManufacturerName { get; set; }

    [JsonPropertyName("manufacturer_country")]
    public string ManufacturerCountry { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long? UpdatedAt { get; set; }

    [JsonPropertyName("__deleted")]
    public string Deleted { get; set; }
}

/// <summary>
/// Handler xử lý command tạo Todo mới
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<List<Guid>>>
{
    // private readonly IKafkaProducer _kafkaProducer;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private string _index;

    /// <summary>
    /// Khởi tạo handler với repository và Kafka producer
    /// </summary>
    /// <param name="todoRepository">Repository xử lý Todo</param>
    /// <param name="kafkaProducer">Producer gửi message đến Kafka</param>
    public CreateProductCommandHandler(IElasticsearchService elasticsearchService)
    {
        _elasticsearchService = elasticsearchService;
        _index = "products_v2";
    }

    /// <summary>
    /// Xử lý command tạo Todo mới
    /// </summary>
    /// <param name="request">Command cần xử lý</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Result chứa ID của Todo mới tạo</returns>
    public async Task<Result<List<Guid>>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            request.QuantityAdd = request.QuantityAdd < 1 ? 100 : request.QuantityAdd;
            var isExitIndex = await _elasticsearchService.IndexExistsAsync(_index);
            if (!isExitIndex)
            {
                var result =  await _elasticsearchService.CreateIndexAsync<ProductDocument>(_index);
                if (result)
                {
                    _logger.LogInformation("Đã tạo chỉ mục '{index}' thành công.", _index);
                }
                else
                {
                    _logger.LogWarning("Không thể tạo chỉ mục '{index}'.",_index);
                    throw new Exception(
                        $"Không thể tạo chỉ mục 'products'.");
                }
            }

            // Tạo faker cho Dimensions
            var dimensionsFaker = new Faker<Dimensions>()
                .RuleFor(d => d.Length, f => f.Random.Double(10, 500))
                .RuleFor(d => d.Width, f => f.Random.Double(10, 500))
                .RuleFor(d => d.Height, f => f.Random.Double(10, 500));

            // Tạo faker cho ManufacturerInfo
            var manufacturerFaker = new Faker<ManufacturerInfo>()
                .RuleFor(m => m.Name, f => f.Company.CompanyName())
                .RuleFor(m => m.Country, f => f.Address.Country());

            // Tạo faker cho ProductDocument
            var productFaker = new Faker<ProductDocument>()
                .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => f.Random.Double(5, 5000))
                .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
                .RuleFor(p => p.Material, f => f.Commerce.ProductMaterial())
                .RuleFor(p => p.ManufacturingDate, f => f.Date.Past(2)) // 2 năm gần đây
                .RuleFor(p => p.Status, f => f.PickRandom(new[] { "Available", "OutOfStock", "Discontinued" }))
                .RuleFor(p => p.Tags, f => f.Random.WordsArray(3, 6).ToList())
                .RuleFor(p => p.Dimensions, f => dimensionsFaker.Generate())
                .RuleFor(p => p.Manufacturer, f => manufacturerFaker.Generate())
                .RuleFor(p => p.CreatedAt, f => DateTime.Now)
                .RuleFor(p => p.UpdatedAt, f => null);

            // Tạo 300 sản phẩm
            var products = productFaker.Generate(request.QuantityAdd);
            var batchCount = 100; // Chia thành 20 tiến trình
            var batchSize = (int)Math.Ceiling((double)products.Count / batchCount);
            var tasks = new List<Task>();
            var productIds = new ConcurrentBag<Guid>();
            var index = 0;
            var lockObj = new object();
            await _elasticsearchService.IndexAllDocumentAsync<ProductDocument>(_index, products);
            // for (int i = 0; i < batchCount; i++)
            // {
            //     var batch = products.Skip(i * batchSize).Take(batchSize).ToList();
            //
            //     tasks.Add(Task.Run(async () =>
            //     {
            //         foreach (var item in batch)
            //         {
            //             var res = await _elasticsearchService.IndexDocumentAsync<ProductDocument>(_index, item, item.Id.ToString());
            //
            //             productIds.Add(Guid.Parse(item.Id));
            //
            //             lock (lockObj)
            //             {
            //                 Console.WriteLine(index++);
            //             }
            //         }
            //     }));
            // }
            //
            // await Task.WhenAll(tasks);
            return Result<List<Guid>>.Success(null);
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, trả về kết quả thất bại với thông báo lỗi
            return Result<List<Guid>>.Failure(new[] { $"Lỗi khi tạo Product: {ex.Message}" });
        }
    }
}
//
// /// <summary>
// /// Event khi Todo được tạo
// /// </summary>
// public class ProductCreatedEvent
// {
//     /// <summary>
//     /// ID của Todo
//     /// </summary>
//     public Guid Id { get; set; }
//     
//     /// <summary>
//     /// Tiêu đề của Todo
//     /// </summary>
//     public string Title { get; set; } = string.Empty;
//     
//     /// <summary>
//     /// Mô tả của Todo
//     /// </summary>
//     public string Description { get; set; } = string.Empty;
//     
//     /// <summary>
//     /// Mức độ ưu tiên của Todo
//     /// </summary>
//     public int Priority { get; set; }
//     
//     /// <summary>
//     /// Ngày đến hạn của Todo
//     /// </summary>
//     public DateTime? DueDate { get; set; }
//     
//     /// <summary>
//     /// Trạng thái hoàn thành của Todo
//     /// </summary>
//     public bool IsCompleted { get; set; }
//     
//     /// <summary>
//     /// Ngày tạo Todo
//     /// </summary>
//     public DateTime CreatedAt { get; set; }
// }
