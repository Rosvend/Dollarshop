using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sales.Infrastructure.Persistence.Configurations;

/// <summary>ORM mapping for the Transactional Outbox table.</summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();

        builder.Property(message => message.MessageType).HasMaxLength(200).IsRequired();
        builder.Property(message => message.RoutingKey).HasMaxLength(200).IsRequired();
        builder.Property(message => message.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.Error).HasMaxLength(2000);

        // The relay polls for pending rows oldest-first.
        builder.HasIndex(message => new { message.ProcessedOn, message.OccurredOn });
    }
}
