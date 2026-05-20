using Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id)
            .HasConversion(Converters.UserId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(user => user.Username)
            .HasConversion(Converters.Username)
            .HasColumnName("username")
            .HasMaxLength(32);

        builder.HasIndex(user => user.Username).IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasConversion(Converters.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(256);

        builder.Property(user => user.Email)
            .HasConversion(Converters.Email)
            .HasColumnName("email")
            .HasMaxLength(256);

        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.Name)
            .HasConversion(Converters.PersonName)
            .HasColumnName("full_name")
            .HasMaxLength(200);

        builder.Property(user => user.Phone)
            .HasConversion(Converters.PhoneNumber)
            .HasColumnName("phone")
            .HasMaxLength(32);

        builder.Property(user => user.Active)
            .HasColumnName("active");

        builder.Ignore(user => user.Profile);
    }
}
