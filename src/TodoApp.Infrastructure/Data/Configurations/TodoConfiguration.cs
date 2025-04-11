using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình cho entity Todo trong database
/// </summary>
public class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    /// <summary>
    /// Cấu hình entity Todo
    /// </summary>
    /// <param name="builder">Builder để cấu hình</param>
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        // Cấu hình bảng
        builder.ToTable("Todos");

        // Cấu hình khóa chính
        builder.HasKey(t => t.Id);

        // Cấu hình thuộc tính Id
        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        // Cấu hình thuộc tính Title
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(100);

        // Cấu hình thuộc tính Description
        builder.Property(t => t.Description)
            .HasMaxLength(500);

        // Cấu hình thuộc tính IsCompleted
        builder.Property(t => t.IsCompleted)
            .IsRequired();

        // Cấu hình thuộc tính Priority
        builder.Property(t => t.Priority)
            .IsRequired();

        // Cấu hình thuộc tính DueDate
        builder.Property(t => t.DueDate)
            .IsRequired(false);

        // Cấu hình thuộc tính CreatedAt
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Cấu hình thuộc tính UpdatedAt
        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);

        // Tạo index trên các trường thường xuyên tìm kiếm
        builder.HasIndex(t => t.IsCompleted);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.DueDate);
    }
}
