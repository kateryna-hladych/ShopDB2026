using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace shopBL_Tests
{
    public class FakeProductHistoryRepository : IProductHistoryRepository
    {
        private readonly List<ProductHistory> _items = new();
        private int _nextId = 1;

        public int AddCalls { get; private set; }

        public IReadOnlyList<ProductHistory> Items => _items;

        public IEnumerable<ProductHistory> GetAll() => _items.ToList();

        public IEnumerable<ProductHistory> GetByProductId(int productId)
            => _items.Where(h => h.ProductId == productId).ToList();

        public ProductHistory GetById(int id) => _items.FirstOrDefault(h => h.Id == id)!;

        public void Add(ProductHistory entity)
        {
            AddCalls++;
            entity.Id = _nextId++;
            _items.Add(entity);
        }

        public void Update(ProductHistory entity) { }

        public void Delete(int id) => _items.RemoveAll(h => h.Id == id);
    }
}