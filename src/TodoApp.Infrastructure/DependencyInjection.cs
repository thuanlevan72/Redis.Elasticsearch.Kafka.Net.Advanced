using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Domain.Interfaces;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Infrastructure.Data;
using TodoApp.Infrastructure.Elasticsearch;
using TodoApp.Infrastructure.Kafka;
using TodoApp.Infrastructure.Repositories;

namespace TodoApp.Infrastructure;

/// <summary>
/// Lớp mở rộng để đăng ký các dependency của tầng Infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các dịch vụ của tầng Infrastructure vào container
    /// </summary>
    /// <param name="services">Container dịch vụ</param>
    /// <param name="configuration">Cấu hình ứng dụng</param>
    /// <returns>Container dịch vụ đã được cấu hình</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký ApplicationDbContext với PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Lấy chuỗi kết nối từ cấu hình
            var connectionString = GetPostgresConnectionString(configuration);
            
            // Cấu hình sử dụng PostgreSQL
            options.UseNpgsql(connectionString, b =>
            {
                // Cấu hình migration
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                // Tăng timeout để tránh lỗi khi thực hiện truy vấn phức tạp
                b.CommandTimeout(60);
                // Cấu hình chuyển đổi ngày giờ để tương thích với PostgreSQL
                b.UseNodaTime();
            });
            
            // Log các truy vấn SQL trong môi trường phát triển
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging", false))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Đăng ký các repository
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoSearchRepository, TodoSearchRepository>();

        // Đăng ký dịch vụ Elasticsearch
        var elasticsearchSection = configuration.GetSection("ElasticsearchSettings");
        services.Configure<ElasticsearchSettings>(options => 
        {
            options.Url = elasticsearchSection["Url"] ?? "http://localhost:9200";
            options.Username = elasticsearchSection["Username"];
            options.Password = elasticsearchSection["Password"];
        });
        services.AddSingleton<IElasticsearchService, ElasticsearchService>(provider =>
        {
            // Lấy cài đặt từ cấu hình
            var settings = new ElasticsearchSettings
            {
                Url = configuration["ElasticsearchSettings:Url"] ?? "http://localhost:9200",
                Username = configuration["ElasticsearchSettings:Username"],
                Password = configuration["ElasticsearchSettings:Password"]
            };
            
            // Tạo dịch vụ với cài đặt
            return new ElasticsearchService(settings);
        });

        // Đăng ký dịch vụ khởi tạo chỉ mục Elasticsearch
        services.AddHostedService<ElasticsearchIndexInitializer>();

        // Đăng ký dịch vụ Kafka
        var kafkaSection = configuration.GetSection("KafkaSettings");
        services.Configure<KafkaSettings>(options => 
        {
            options.BootstrapServers = kafkaSection["BootstrapServers"] ?? "localhost:9092";
            options.TopicPrefix = kafkaSection["TopicPrefix"] ?? "todo";
            options.GroupId = kafkaSection["GroupId"] ?? "todo-consumer-group";
        });
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddHostedService<KafkaConsumer>();

        return services;
    }

    /// <summary>
    /// Lấy chuỗi kết nối PostgreSQL từ cấu hình hoặc biến môi trường
    /// </summary>
    /// <param name="configuration">Cấu hình ứng dụng</param>
    /// <returns>Chuỗi kết nối PostgreSQL</returns>
    private static string GetPostgresConnectionString(IConfiguration configuration)
    {
        // Ưu tiên sử dụng DATABASE_URL nếu có (phổ biến trên các nền tảng PaaS)
        var databaseUrl = configuration.GetValue<string>("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            // Phân tích URI để lấy thông tin kết nối
            return ConvertDatabaseUrlToConnectionString(databaseUrl);
        }

        // Nếu không có DATABASE_URL, lấy từ cấu hình hoặc biến môi trường riêng lẻ
        var host = configuration.GetValue<string>("PGHOST") 
                  ?? configuration.GetValue<string>("PostgreSQL:Host") 
                  ?? "localhost";
        
        var port = configuration.GetValue<string>("PGPORT") 
                  ?? configuration.GetValue<string>("PostgreSQL:Port") 
                  ?? "5432";
        
        var database = configuration.GetValue<string>("PGDATABASE") 
                      ?? configuration.GetValue<string>("PostgreSQL:Database") 
                      ?? "tododb";
        
        var username = configuration.GetValue<string>("PGUSER") 
                      ?? configuration.GetValue<string>("PostgreSQL:Username") 
                      ?? "postgres";
        
        var password = configuration.GetValue<string>("PGPASSWORD") 
                      ?? configuration.GetValue<string>("PostgreSQL:Password") 
                      ?? "postgres";

        // Xây dựng chuỗi kết nối
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    }

    /// <summary>
    /// Chuyển đổi DATABASE_URL thành chuỗi kết nối PostgreSQL
    /// </summary>
    /// <param name="databaseUrl">DATABASE_URL</param>
    /// <returns>Chuỗi kết nối PostgreSQL</returns>
    private static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
    {
        try
        {
            // Phân tích URI
            var uri = new Uri(databaseUrl);
            
            // Lấy thông tin từ URI
            var host = uri.Host;
            
            // Sử dụng cổng mặc định nếu cổng không hợp lệ
            var port = uri.Port > 0 ? uri.Port : 5432;
            
            var database = uri.AbsolutePath.TrimStart('/');
            
            // Phân tích thông tin xác thực
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;

            // Lấy ssl mode từ query string nếu có
            var sslMode = "Require"; // Mặc định cho cloud database
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (query["sslmode"] != null)
            {
                sslMode = query["sslmode"];
            }
            
            // Xây dựng chuỗi kết nối với SSL mode
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode};";
        }
        catch (Exception)
        {
            // Nếu có lỗi, trả về chuỗi kết nối mặc định
            return databaseUrl;
        }
    }
}
