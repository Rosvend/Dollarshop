namespace Identity.Domain.Common;

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
