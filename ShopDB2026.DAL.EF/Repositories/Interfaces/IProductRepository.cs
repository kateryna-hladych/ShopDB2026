using ShopDB.DAL.EF.Entities;

namespace ShopDB.DAL.EF.Repositories.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    IEnumerable<Product> GetActiveProducts();
}