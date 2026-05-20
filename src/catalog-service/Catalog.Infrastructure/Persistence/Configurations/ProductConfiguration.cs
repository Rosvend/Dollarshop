using Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id)
            .HasConversion(Converters.ProductId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(product => product.Sku)
            .HasConversion(Converters.Sku)
            .HasColumnName("sku")
            .HasMaxLength(64);

        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.Name)
            .HasConversion(Converters.ProductName)
            .HasColumnName("name")
            .HasMaxLength(200);

        builder.Property(product => product.ListPrice)
            .HasConversion(Converters.Money)
            .HasColumnName("list_price")
            .HasMaxLength(32);

        builder.Property(product => product.Stock)
            .HasConversion(Converters.StockLevel)
            .HasColumnName("stock_on_hand");
    }
}
