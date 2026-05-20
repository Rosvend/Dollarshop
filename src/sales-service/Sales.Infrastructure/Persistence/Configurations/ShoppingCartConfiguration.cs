using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Aggregates;
using Sales.Domain.ValueObjects.Discounts;

namespace Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// ORM mapping for the <see cref="ShoppingCart"/> aggregate root. EF Core
/// materializes the cart through its public constructor (<c>id</c>, <c>owner</c> —
/// both value-converted scalars) and writes the remaining state through the
/// private setter / backing fields. The Domain class is never touched.
/// </summary>
internal sealed class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(cart => cart.Id);
        builder.Property(cart => cart.Id)
            .HasConversion(Converters.CartId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(cart => cart.Owner)
            .HasConversion(Converters.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(cart => cart.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20);

        // The polymorphic discount policy collapses to one jsonb column.
        builder.Property<DiscountPolicy>("_policy")
            .HasConversion(Converters.DiscountPolicy)
            .HasColumnName("discount")
            .HasColumnType("jsonb")
            .IsRequired(false);
        builder.Ignore(cart => cart.Discount);

        // Domain events are transient — flushed to the Outbox, never persisted on the cart.
        builder.Ignore(cart => cart.DomainEvents);

        // The cart owns its items; EF reads/writes the private _items backing field.
        builder.HasMany(cart => cart.Items)
            .WithOne()
            .HasForeignKey("CartId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata
            .FindNavigation(nameof(ShoppingCart.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
