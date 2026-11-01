using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace shopBL_Tests
{
    public class FakeProductRepository : IProductRepository
    {
        private readonly List<Product> _items = new();
        private int _nextId = 1;

        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int DeleteCalls { get; private set; }

        public IReadOnlyList<Product> Items => _items;

        public IEnumerable<Product> GetAll() => _items.ToList();

        public IEnumerable<Product> GetActiveProducts() => _items.Where(p => !p.IsBlocked).ToList();

        public Product GetById(int id) => _items.FirstOrDefault(p => p.ProductId == id)!;

        public void Add(Product entity)
        {
            AddCalls++;
            entity.ProductId = _nextId++;
            _items.Add(entity);
        }

        public void Update(Product entity) => UpdateCalls++;

        public void Delete(int id)
        {
            DeleteCalls++;
            _items.RemoveAll(p => p.ProductId == id);
        }
    }
}