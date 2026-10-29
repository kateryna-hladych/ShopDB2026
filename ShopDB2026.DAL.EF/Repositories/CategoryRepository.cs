using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopDB.DAL.EF.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ShopDbContext context) : base(context) { }
}