using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopDB.DAL.EF.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ShopDbContext _context;

    public BaseRepository(ShopDbContext context) => _context = context;

    public IEnumerable<T> GetAll() => _context.Set<T>().ToList();

    public T GetById(int id) => _context.Set<T>().Find(id);

    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var entity = GetById(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }
    }
}