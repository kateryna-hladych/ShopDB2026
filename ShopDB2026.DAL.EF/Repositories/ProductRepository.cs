using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopDB.DAL.EF.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ShopDbContext context) : base(context) { }

    public IEnumerable<Product> GetActiveProducts() =>
        _context.Products.Where(p => !p.IsBlocked).ToList();
}