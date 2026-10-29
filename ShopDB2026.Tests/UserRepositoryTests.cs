using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories;

namespace ShopDB.Tests
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private ShopDbContext _context = null!;
        private UserRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ShopDbContext(options);
            _repository = new UserRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void GetByUsername_WhenUserExists_ReturnsUser()
        {
            _context.Users.Add(new User { UserId = 1, Username = "admin", PasswordHash = "hash123", Role = "Admin" });
            _context.SaveChanges();

            var user = _repository.GetByUsername("admin");

            Assert.That(user, Is.Not.Null);
            Assert.That(user!.Username, Is.EqualTo("admin"));
        }
    }
}