using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Models;

namespace TodoApp.Infrastructure.Elasticsearch;

/// <summary>
/// Dịch vụ khởi tạo chỉ mục Elasticsearch khi ứng dụng khởi động
/// </summary>
public class ElasticsearchIndexInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ElasticsearchIndexInitializer> _logger;

    /// <summary>
    /// Khởi tạo dịch vụ với service provider và logger
    /// </summary>
    /// <param name="serviceProvider">Service provider để lấy dịch vụ</param>
    /// <param name="logger">Logger</param>
    public ElasticsearchIndexInitializer(
        IServiceProvider serviceProvider,
        ILogger<ElasticsearchIndexInitializer> logger)
    {
        // Lưu trữ service provider
        _serviceProvider = serviceProvider;
        // Lưu trữ logger
        _logger = logger;
    }

    /// <summary>
    /// Khởi động dịch vụ, được gọi khi ứng dụng khởi động
    /// </summary>
    /// <param name="cancellationToken">Token hủy</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Khởi tạo chỉ mục trong luồng riêng, không chặn việc khởi động ứng dụng
        Task.Run(async () => await InitializeIndexAsync(), cancellationToken);
        
        // Trả về ngay task hoàn thành để cho phép ứng dụng tiếp tục khởi động
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Phương thức nội bộ thực hiện việc khởi tạo chỉ mục Elasticsearch
    /// </summary>
    private async Task InitializeIndexAsync()
    {
        try
        {
            await Task.Delay(500); // Đợi ứng dụng khởi động trước khi tiếp tục
            
            // Tạo scope để lấy dịch vụ
            using var scope = _serviceProvider.CreateScope();
            
            // Lấy dịch vụ Elasticsearch
            var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

            try
            {
                // Kiểm tra xem chỉ mục todos đã tồn tại chưa
                _logger.LogInformation("Đang kiểm tra chỉ mục Elasticsearch...");
                
                var indexExists = await elasticsearchService.IndexExistsAsync("todos");
                
                // Nếu chỉ mục chưa tồn tại, tạo mới
                if (!indexExists)
                {
                    _logger.LogInformation("Chỉ mục 'todos' không tồn tại. Đang tạo chỉ mục...");
                    
                    var result = await elasticsearchService.CreateIndexAsync<TodoDocument>("todos");
                    
                    if (result)
                    {
                        _logger.LogInformation("Đã tạo chỉ mục 'todos' thành công.");
                    }
                    else
                    {
                        _logger.LogWarning("Không thể tạo chỉ mục 'todos'. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng tìm kiếm có thể bị ảnh hưởng.");
                    }
                }
                else
                {
                    _logger.LogInformation("Chỉ mục 'todos' đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không dừng ứng dụng
                _logger.LogWarning(ex, "Xảy ra lỗi khi khởi tạo chỉ mục Elasticsearch. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng tìm kiếm có thể bị ảnh hưởng.");
            }
        }
        catch (Exception ex)
        {
            // Log lỗi khi không thể lấy dịch vụ
            _logger.LogWarning(ex, "Không thể khởi tạo dịch vụ Elasticsearch. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng tìm kiếm có thể bị ảnh hưởng.");
        }
    }

    /// <summary>
    /// Dừng dịch vụ, được gọi khi ứng dụng dừng
    /// </summary>
    /// <param name="cancellationToken">Token hủy</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Không cần làm gì khi dừng
        return Task.CompletedTask;
    }
}
