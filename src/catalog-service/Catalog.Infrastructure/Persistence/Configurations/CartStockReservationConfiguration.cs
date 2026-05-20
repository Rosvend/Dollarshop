using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CartStockReservationConfiguration : IEntityTypeConfiguration<CartStockReservation>
{
    public void Configure(EntityTypeBuilder<CartStockReservation> builder)
    {
        builder.ToTable("cart_stock_reservations");

        builder.HasKey(row => new { row.CartId, row.ProductId });

        builder.Property(row => row.CartId).HasColumnName("cart_id");
        builder.Property(row => row.ProductId).HasColumnName("product_id");
        builder.Property(row => row.Quantity).HasColumnName("quantity");
    }
}
