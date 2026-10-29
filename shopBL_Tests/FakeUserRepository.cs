using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace shopBL_Tests
{
    public class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public int AddCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int LookupCalls { get; private set; }

        public IReadOnlyList<User> Users => _users;

        public IEnumerable<User> GetAll() => _users.ToList();

        public User GetById(int id) => _users.FirstOrDefault(u => u.UserId == id)!;

        public User? GetByUsername(string username)
        {
            LookupCalls++;
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public void Add(User entity)
        {
            AddCalls++;
            entity.UserId = _nextId++;
            _users.Add(entity);
        }

        public void Update(User entity)
        {
            UpdateCalls++;
        }

        public void Delete(int id)
        {
            _users.RemoveAll(u => u.UserId == id);
        }
    }
}