using ProiectMpp.Domain;

namespace ProiectMpp.Repository;

public interface IUserRepository:IRepository<int,User>
{
    public User? FindByUsername(string username);
}