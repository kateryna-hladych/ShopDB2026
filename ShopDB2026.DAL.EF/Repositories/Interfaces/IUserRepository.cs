using ShopDB.DAL.EF.Entities;

namespace ShopDB.DAL.EF.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByUsername(string username);
    }
}