using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ShopDB.Tests
{
    [TestFixture]
    public class ProductRepositoryTests
    {
        private ShopDbContext _context;
        private ProductRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ShopDbContext(options);
            _repository = new ProductRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void GetAll_ShouldReturnAllProducts()
        {
            _context.Products.Add(new Product { ProductId = 1, Name = "P1" });
            _context.Products.Add(new Product { ProductId = 2, Name = "P2" });
            _context.SaveChanges();

            var result = _repository.GetAll();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetById_ShouldReturnProduct_WhenExists()
        {
            _context.Products.Add(new Product { ProductId = 1, Name = "Мишка" });
            _context.SaveChanges();

            var result = _repository.GetById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Мишка"));
        }

        [Test]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            var result = _repository.GetById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Add_ShouldAddProductToDb()
        {
            var product = new Product { ProductId = 1, Name = "Клавіатура", SellingPrice = 50 };

            _repository.Add(product);

            var saved = _context.Products.Find(1);
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Name, Is.EqualTo("Клавіатура"));
        }

        [Test]
        public void Add_ShouldThrowException_WhenNull()
        {
            Assert.That(() => _repository.Add(null!), Throws.Exception);
        }

        [Test]
        public void Update_ShouldUpdateProductInDb()
        {
            var product = new Product { ProductId = 1, Name = "Стара назва", SellingPrice = 10 };
            _context.Products.Add(product);
            _context.SaveChanges();

            product.Name = "Нова назва";
            product.SellingPrice = 20;
            _repository.Update(product);

            var updated = _context.Products.Find(1);
            Assert.That(updated!.Name, Is.EqualTo("Нова назва"));
            Assert.That(updated.SellingPrice, Is.EqualTo(20));
        }

        [Test]
        public void Delete_ShouldRemoveProductFromDb()
        {
            var product = new Product { ProductId = 1, Name = "На видалення" };
            _context.Products.Add(product);
            _context.SaveChanges();

            _repository.Delete(1);

            var deleted = _context.Products.Find(1);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Delete_ShouldNotFail_WhenIdNotExists()
        {
            Assert.That(() => _repository.Delete(999), Throws.Nothing);
        }

        [Test]
        public void GetActiveProducts_ShouldReturnOnlyNonBlockedProducts()
        {
            _context.Products.Add(new Product { ProductId = 1, Name = "Активний товар", IsBlocked = false });
            _context.Products.Add(new Product { ProductId = 2, Name = "Заблокований товар", IsBlocked = true });
            _context.SaveChanges();

            var result = _repository.GetActiveProducts();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Активний товар"));
        }
    }
}