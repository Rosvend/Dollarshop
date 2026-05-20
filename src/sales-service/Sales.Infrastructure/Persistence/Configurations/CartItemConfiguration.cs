using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Aggregates;
using Sales.Domain.ValueObjects;

namespace Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// ORM mapping for the <see cref="CartItem"/> entity. <c>CartItem</c> exposes only
/// an internal three-argument constructor; mapping <see cref="CartItem.Product"/>
/// (the <c>ProductReference</c> Value Object) as a jsonb value-converted scalar —
/// rather than an EF owned type — keeps every constructor parameter scalar, so EF
/// can bind that constructor (owned types cannot be constructor-bound).
/// </summary>
internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .HasConversion(Converters.CartItemId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.Quantity)
            .HasConversion(Converters.Quantity)
            .HasColumnName("quantity");

        builder.Property(item => item.Product)
            .HasConversion(Converters.ProductReference)
            .HasColumnName("product")
            .HasColumnType("jsonb");

        // Shadow foreign key to the owning cart. Its CLR type must match the
        // ShoppingCart principal key (CartId), so it carries the same converter.
        builder.Property<CartId>("CartId")
            .HasConversion(Converters.CartId)
            .HasColumnName("cart_id");
        builder.HasIndex("CartId");
    }
}
