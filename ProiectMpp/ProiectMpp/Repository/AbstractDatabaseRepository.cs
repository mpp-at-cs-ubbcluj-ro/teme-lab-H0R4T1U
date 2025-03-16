using ProiectMpp.Domain;
using log4net;

namespace ProiectMpp.Repository
{
    abstract public class AbstractDatabaseRepository<TId, TE> : IRepository<TId, TE> where TE : Entity<TId>
    {
        protected IDictionary<TId, TE> Data;
        protected static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDBRepository));
        protected readonly string ConnectionString;

        public AbstractDatabaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
            Data = new Dictionary<TId, TE>();
        }

        abstract public TE? FindOne(TId id);

        public IDictionary<TId, TE> FindAll()
        { 
            return Data;
        }

        abstract protected void Load();
        abstract public TE? Save(TE entity);
        abstract public TE? Delete(TId id);
        abstract public TE? Update(TE entity);
    }
}