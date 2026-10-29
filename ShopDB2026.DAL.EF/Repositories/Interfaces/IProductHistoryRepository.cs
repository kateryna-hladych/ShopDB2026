using ShopDB.DAL.EF.Entities;

namespace ShopDB.DAL.EF.Repositories.Interfaces;

public interface IProductHistoryRepository : IRepository<ProductHistory>
{
    IEnumerable<ProductHistory> GetByProductId(int productId);
}