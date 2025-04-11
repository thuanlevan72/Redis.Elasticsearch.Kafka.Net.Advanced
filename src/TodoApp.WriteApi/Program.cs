using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoApp.Application;
using TodoApp.Infrastructure;
using TodoApp.Infrastructure.Data;

// Khởi tạo cấu hình logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // Log thông tin khởi động ứng dụng
    Log.Information("Đang khởi động Write API");

    // Khởi tạo builder
    var builder = WebApplication.CreateBuilder(args);

    // Cấu hình logging
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Thêm services cho controllers
    builder.Services.AddControllers();

    // Thêm services cho Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { 
            Title = "TodoApp.WriteApi", 
            Version = "v1",
            Description = "API ghi dữ liệu cho ứng dụng Todo (Write API)",
            Contact = new() {
                Name = "Todo App Team",
                Email = "support@todoapp.com"
            }
        });
        
        // Thêm chú thích XML cho Swagger
        var xmlFile = $"TodoApp.WriteApi.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    // Thêm các services từ các tầng khác
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Thêm CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });

    // Xây dựng ứng dụng
    var app = builder.Build();

    // Cấu hình pipeline request
    // Luôn sử dụng Swagger để dễ dàng test API
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Tự động áp dụng migration
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Log.Information("Đã áp dụng migrations cho database");
    }

    // Sử dụng CORS
    app.UseCors("AllowAllOrigins");

    // Sử dụng HTTPS redirection
    app.UseHttpsRedirection();

    // Sử dụng routing và endpoints
    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();

    // Chạy ứng dụng
    app.Run();
}
catch (Exception ex)
{
    // Log lỗi không mong đợi
    Log.Fatal(ex, "Ứng dụng bị dừng không mong muốn");
}
finally
{
    // Đảm bảo giải phóng tài nguyên logging
    Log.CloseAndFlush();
}
