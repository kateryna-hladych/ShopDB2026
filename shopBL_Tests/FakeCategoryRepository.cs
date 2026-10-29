using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace shopBL_Tests
{
    public class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _items = new();
        private int _nextId = 1;

        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int DeleteCalls { get; private set; }

        public IReadOnlyList<Category> Items => _items;

        public IEnumerable<Category> GetAll() => _items.ToList();

        public Category GetById(int id) => _items.FirstOrDefault(c => c.CategoryId == id)!;

        public void Add(Category entity)
        {
            AddCalls++;
            entity.CategoryId = _nextId++;
            _items.Add(entity);
        }

        public void Update(Category entity) => UpdateCalls++;

        public void Delete(int id)
        {
            DeleteCalls++;
            _items.RemoveAll(c => c.CategoryId == id);
        }
    }
}