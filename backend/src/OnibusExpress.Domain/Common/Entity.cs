namespace OnibusExpress.Domain.Common;

/// <summary>Base entity with identity by <see cref="Id"/> (equality by Id + type).</summary>
public abstract class Entity
{
    public Guid Id { get; protected init; }

    public override bool Equals(object? obj) =>
        obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}
