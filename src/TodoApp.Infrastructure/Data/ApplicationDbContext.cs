using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Data.Configurations;

namespace TodoApp.Infrastructure.Data;

/// <summary>
/// Context database cho ứng dụng
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// DbSet cho entity Todo
    /// </summary>
    public DbSet<Todo> Todos { get; set; } = null!;
    
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Khởi tạo context với options được cung cấp
    /// </summary>
    /// <param name="options">Options cho context</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    /// <summary>
    /// Cấu hình model khi xây dựng
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder để cấu hình model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Gọi phương thức của lớp cha
        base.OnModelCreating(modelBuilder);

        // Áp dụng cấu hình cho entity Todo
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
