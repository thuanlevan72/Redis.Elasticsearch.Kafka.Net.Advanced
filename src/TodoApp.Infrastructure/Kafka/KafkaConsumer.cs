using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MediatR;
using TodoApp.Application.Todos.Commands.CreateTodo;
using TodoApp.Application.Todos.Commands.UpdateTodo;
using TodoApp.Application.Todos.Commands.DeleteTodo;
using TodoApp.Application.Todos.EventHandlers;

namespace TodoApp.Infrastructure.Kafka;

/// <summary>
/// Dịch vụ nghe và xử lý sự kiện từ Kafka
/// </summary>
public class KafkaConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumer> _logger;

    /// <summary>
    /// Khởi tạo consumer với cài đặt, service provider và logger
    /// </summary>
    /// <param name="settings">Cài đặt Kafka</param>
    /// <param name="serviceProvider">Service provider để lấy dịch vụ</param>
    /// <param name="logger">Logger</param>
    public KafkaConsumer(
        IOptions<KafkaSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumer> logger)
    {
        // Lưu trữ cài đặt
        _settings = settings.Value;
        // Lưu trữ service provider
        _serviceProvider = serviceProvider;
        // Lưu trữ logger
        _logger = logger;
    }

    /// <summary>
    /// Thực thi consumer trong nền
    /// </summary>
    /// <param name="stoppingToken">Token hủy</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Log thông tin bắt đầu
        _logger.LogInformation("Kafka Consumer đang khởi động...");
        //
        // // Khởi động consumer trong một task riêng không chặn việc khởi động ứng dụng
        // _ = Task.Run(async () => await StartKafkaConsumerAsync(stoppingToken), stoppingToken);
        // _ = Task.Run(async () => await StartKafkaConsumerCDCAsync(stoppingToken), stoppingToken);
        // Khởi chạy CẢ HAI consumers trong các Task riêng biệt
        var domainConsumerTask = Task.Run(() => 
            StartKafkaConsumerAsync(stoppingToken), stoppingToken);
    
        var cdcConsumerTask = Task.Run(() => 
            StartKafkaConsumerCDCAsync(stoppingToken), stoppingToken);

        // Đợi cả 2 task hoàn thành (sẽ chạy vô tận cho đến khi ứng dụng dừng)
        await Task.WhenAll(domainConsumerTask, cdcConsumerTask);
    }

    private async Task StartKafkaConsumerCDCAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Đợi một chút để ứng dụng có thể khởi động trước
            await Task.Delay(500, stoppingToken);

            // Cấu hình consumer
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.CDCGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                // Thêm cấu hình để xử lý tốt hơn với lỗi kết nối
                SocketTimeoutMs = 3000,
                ConnectionsMaxIdleMs = 5000,
                AllowAutoCreateTopics = true
            };

            try
            {
                // Tạo consumer
                using var consumer = new ConsumerBuilder<string, string>(config)
                    .SetErrorHandler((_, e) => 
                    {
                        if (e.IsFatal)
                        {
                            _logger.LogError($"Lỗi nghiêm trọng từ Kafka consumer: {e.Reason}");
                        }
                        else
                        {
                            _logger.LogWarning($"Lỗi không nghiêm trọng từ Kafka consumer: {e.Reason}");
                        }
                    })
                    .Build();
                
                try
                {
                    // Đăng ký nhận thông tin từ topic
                    consumer.Subscribe(_settings.CDCEventsTopic);
                    
                    // Log thông tin đã đăng ký
                    _logger.LogInformation($"Đã đăng ký nhận thông tin từ topic: {_settings.CDCEventsTopic}");
                    
                    // Vòng lặp xử lý message
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Đọc message từ Kafka với timeout
                            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(100));
                            
                            // Nếu có message
                            if (consumeResult != null)
                            {
                                // Log thông tin nhận được message
                                _logger.LogInformation($"Đã nhận message từ partition {consumeResult.Partition.Value} với offset {consumeResult.Offset.Value}");
                                
                                // Commit offset
                                consumer.Commit(consumeResult);
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            // Log lỗi khi consume
                            _logger.LogError($"Lỗi khi nhận message từ Kafka: {ex.Error.Reason}");
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi không mong đợi khi xử lý message
                            _logger.LogError(ex, "Lỗi không mong đợi khi xử lý message từ Kafka.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không dừng ứng dụng
                    _logger.LogWarning(ex, $"Không thể đăng ký nhận thông tin từ topic '{_settings.TodoEventsTopic}'. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng đồng bộ dữ liệu có thể bị ảnh hưởng.");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi khi tạo consumer
                _logger.LogWarning(ex, "Không thể tạo Kafka consumer. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng đồng bộ dữ liệu có thể bị ảnh hưởng.");
            }
        }
        catch (OperationCanceledException)
        {
            // Bình thường khi ứng dụng dừng
            _logger.LogInformation("Kafka Consumer đã dừng theo yêu cầu.");
        }
        catch (Exception ex)
        {
            // Log lỗi không mong đợi
            _logger.LogWarning(ex, "Lỗi không mong đợi trong Kafka Consumer. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng đồng bộ dữ liệu có thể bị ảnh hưởng.");
        }

        // Đảm bảo ExecuteAsync không kết thúc khi có lỗi, để ứng dụng tiếp tục chạy
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            _logger.LogDebug("Kafka Consumer service đang chạy...");
        }
    }
    
    

    /// <summary>
    /// Khởi động và chạy Kafka consumer
    /// </summary>
    /// <param name="stoppingToken">Token hủy</param>
    private async Task StartKafkaConsumerAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Đợi một chút để ứng dụng có thể khởi động trước
            await Task.Delay(500, stoppingToken);

            // Cấu hình consumer
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                // Thêm cấu hình để xử lý tốt hơn với lỗi kết nối
                SocketTimeoutMs = 3000,
                ConnectionsMaxIdleMs = 5000,
                AllowAutoCreateTopics = true
            };

            try
            {
                // Tạo consumer
                using var consumer = new ConsumerBuilder<string, string>(config)
                    .SetErrorHandler((_, e) => 
                    {
                        if (e.IsFatal)
                        {
                            _logger.LogError($"Lỗi nghiêm trọng từ Kafka consumer: {e.Reason}");
                        }
                        else
                        {
                            _logger.LogWarning($"Lỗi không nghiêm trọng từ Kafka consumer: {e.Reason}");
                        }
                    })
                    .Build();
                
                try
                {
                    // Đăng ký nhận thông tin từ topic
                    consumer.Subscribe(_settings.TodoEventsTopic);
                    
                    // Log thông tin đã đăng ký
                    _logger.LogInformation($"Đã đăng ký nhận thông tin từ topic: {_settings.TodoEventsTopic}");
                    
                    // Vòng lặp xử lý message
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Đọc message từ Kafka với timeout
                            var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(100));
                            
                            // Nếu có message
                            if (consumeResult != null)
                            {
                                // Log thông tin nhận được message
                                _logger.LogInformation($"Đã nhận message từ partition {consumeResult.Partition.Value} với offset {consumeResult.Offset.Value}");
                                
                                // Xử lý message
                                await ProcessMessage(consumeResult.Message.Key, consumeResult.Message.Value, stoppingToken);
                                
                                // Commit offset
                                consumer.Commit(consumeResult);
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            // Log lỗi khi consume
                            _logger.LogError($"Lỗi khi nhận message từ Kafka: {ex.Error.Reason}");
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi không mong đợi khi xử lý message
                            _logger.LogError(ex, "Lỗi không mong đợi khi xử lý message từ Kafka.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không dừng ứng dụng
                    _logger.LogWarning(ex, $"Không thể đăng ký nhận thông tin từ topic '{_settings.TodoEventsTopic}'. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng đồng bộ dữ liệu có thể bị ảnh hưởng.");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi khi tạo consumer
                _logger.LogWarning(ex, "Không thể tạo Kafka consumer. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng đồng bộ dữ liệu có thể bị ảnh hưởng.");
            }
        }
        catch (OperationCanceledException)
        {
            // Bình thường khi ứng dụng dừng
            _logger.LogInformation("Kafka Consumer đã dừng theo yêu cầu.");
        }
        catch (Exception ex)
        {
            // Log lỗi không mong đợi
            _logger.LogWarning(ex, "Lỗi không mong đợi trong Kafka Consumer. Ứng dụng sẽ tiếp tục chạy, nhưng tính năng đồng bộ dữ liệu có thể bị ảnh hưởng.");
        }

        // Đảm bảo ExecuteAsync không kết thúc khi có lỗi, để ứng dụng tiếp tục chạy
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            _logger.LogDebug("Kafka Consumer service đang chạy...");
        }
    }

    /// <summary>
    /// Xử lý message nhận được từ Kafka
    /// </summary>
    /// <param name="key">Khóa của message</param>
    /// <param name="value">Giá trị của message</param>
    /// <param name="cancellationToken">Token hủy</param>
    private async Task ProcessMessage(string key, string value, CancellationToken cancellationToken)
    {
        try
        {
            // Kiểm tra nội dung message
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning("Nhận được message rỗng, bỏ qua.");
                return;
            }

            // Tạo scope để lấy dịch vụ
            using var scope = _serviceProvider.CreateScope();
            
            // Lấy mediator
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Phân tích nội dung message để xác định loại sự kiện
            var messageDocument = JsonDocument.Parse(value);
            var root = messageDocument.RootElement;

            // Kiểm tra sự kiện dựa trên các thuộc tính có trong message
            bool isCreatedEvent = root.TryGetProperty("CreatedAt", out _) && !root.TryGetProperty("UpdatedAt", out _);
            bool isUpdatedEvent = root.TryGetProperty("UpdatedAt", out _);
            bool isDeletedEvent = root.TryGetProperty("Id", out _) && root.EnumerateObject().Count() == 1;

            // Xử lý theo loại sự kiện
            if (isCreatedEvent)
            {
                // Parse sự kiện Todo đã tạo
                var todoCreatedEvent = JsonSerializer.Deserialize<TodoCreatedEvent>(value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (todoCreatedEvent != null)
                {
                    // Tạo notification và gửi đến handler
                    await mediator.Publish(new TodoCreatedNotification(todoCreatedEvent), cancellationToken);
                    _logger.LogInformation($"Đã xử lý sự kiện Todo đã tạo: {todoCreatedEvent.Id}");
                }
            }
            else if (isUpdatedEvent)
            {
                // Parse sự kiện Todo đã cập nhật
                var todoUpdatedEvent = JsonSerializer.Deserialize<TodoUpdatedEvent>(value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (todoUpdatedEvent != null)
                {
                    // Tạo notification và gửi đến handler
                    await mediator.Publish(new TodoUpdatedNotification(todoUpdatedEvent), cancellationToken);
                    _logger.LogInformation($"Đã xử lý sự kiện Todo đã cập nhật: {todoUpdatedEvent.Id}");
                }
            }
            else if (isDeletedEvent)
            {
                // Parse sự kiện Todo đã xóa
                var todoDeletedEvent = JsonSerializer.Deserialize<TodoDeletedEvent>(value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (todoDeletedEvent != null)
                {
                    // Tạo notification và gửi đến handler
                    await mediator.Publish(new TodoDeletedNotification(todoDeletedEvent), cancellationToken);
                    _logger.LogInformation($"Đã xử lý sự kiện Todo đã xóa: {todoDeletedEvent.Id}");
                }
            }
            else
            {
                // Log cảnh báo nếu không xác định được loại sự kiện
                _logger.LogWarning($"Không thể xác định loại sự kiện từ message: {value}");
            }
        }
        catch (Exception ex)
        {
            // Log lỗi khi xử lý message
            _logger.LogError(ex, $"Lỗi khi xử lý message: {value}");
        }
    }
}
