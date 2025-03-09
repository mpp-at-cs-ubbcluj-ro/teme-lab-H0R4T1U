using ProiectMpp.Domain;

namespace ProiectMpp.Repository;

public interface IRepository<in TId, TE> where TE : Entity<TId>
{
    TE FindOne(TId id);
    IEnumerable<TE> FindAll();
    TE Save(TE entity);
    TE Delete(TId id);
    TE Update(TE entity);
}