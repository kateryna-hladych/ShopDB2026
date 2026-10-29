using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopDB.DAL.EF.Repositories;

public class ProductHistoryRepository : BaseRepository<ProductHistory>, IProductHistoryRepository
{
    public ProductHistoryRepository(ShopDbContext context) : base(context) { }

    
 public IEnumerable<ProductHistory> GetByProductId(int productId)
    {
        return _context.ProductHistories
            .Where(h => h.ProductId == productId)
            .ToList();
    }
}