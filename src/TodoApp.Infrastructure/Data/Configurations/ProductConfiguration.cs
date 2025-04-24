using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình cho entity Todo trong database
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Cấu hình entity Todo
    /// </summary>
    /// <param name="builder">Builder để cấu hình</param>
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        
        entity.ToTable("products");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name).HasColumnName("name");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.Price).HasColumnName("price");
        entity.Property(e => e.Category).HasColumnName("category");
        entity.Property(e => e.Material).HasColumnName("material");
        entity.Property(e => e.ManufacturingDate)
            .HasColumnType("timestamp with time zone").HasColumnName("manufacturing_date");
        entity.Property(e => e.Status).HasColumnName("status");

        entity.OwnsOne(e => e.Dimensions, dim =>
        {
            dim.Property(d => d.Length).HasColumnName("dimensions_length");
            dim.Property(d => d.Width).HasColumnName("dimensions_width");
            dim.Property(d => d.Height).HasColumnName("dimensions_height");
        });

        entity.Property(e => e.Tags).HasColumnName("tags");

        entity.OwnsOne(e => e.Manufacturer, manu =>
        {
            manu.Property(m => m.Name).HasColumnName("manufacturer_name");
            manu.Property(m => m.Country).HasColumnName("manufacturer_country");
        });

        entity.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");
        entity.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");
    }
}
