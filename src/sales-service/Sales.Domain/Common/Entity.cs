namespace Sales.Domain.Common;

/// <summary>
/// Base class for Entities: objects with a stable identity. Two entities are
/// equal when they share the same identity, regardless of their attributes.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the entity.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; }

    protected Entity(TId id)
    {
        Id = id ?? throw new DomainException("An entity requires a non-null identity.");
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => Id.GetHashCode();
}
