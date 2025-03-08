namespace ProiectMpp.Domain;

public class Entity<ID>
{
    protected ID id;
    public ID getId() => id;
    public void SetId(ID id) => this.id = id;

    protected bool Equals(Entity<ID> other)
    {
        return EqualityComparer<ID>.Default.Equals(id, other.id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity<ID>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<ID>.Default.GetHashCode(id);
    }
}