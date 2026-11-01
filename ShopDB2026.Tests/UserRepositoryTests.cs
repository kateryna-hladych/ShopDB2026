
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

        [Test]
        public void GetByUsername_WhenUserDoesNotExist_ReturnsNull()
        {
            var user = _repository.GetByUsername("ghost");

            Assert.That(user, Is.Null);
        }

        [Test]
        public void Add_NewUser_SavesUserWithRoleAndHash()
        {
            _repository.Add(new User { Username = "manager", PasswordHash = "hash456", Role = "ProductManager" });

            var saved = _context.Users.Single();
            Assert.That(saved.Username, Is.EqualTo("manager"));
            Assert.That(saved.PasswordHash, Is.EqualTo("hash456"));
            Assert.That(saved.Role, Is.EqualTo("ProductManager"));
        }

        [Test]
        public void Update_ChangesPasswordHash()
        {
            var user = new User { Username = "ivan", PasswordHash = "old", Role = "User" };
            _repository.Add(user);

            user.PasswordHash = "new";
            _repository.Update(user);

            Assert.That(_repository.GetByUsername("ivan")!.PasswordHash, Is.EqualTo("new"));
        }

        [Test]
        public void Delete_RemovesUser()
        {
            var user = new User { Username = "ivan", PasswordHash = "hash", Role = "User" };
            _repository.Add(user);

            _repository.Delete(user.UserId);

            Assert.That(_context.Users.Count(), Is.EqualTo(0));
        }
    }
}