using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ShopDB.Tests
{
    [TestFixture]
    public class ProductHistoryRepositoryTests
    {
        private ShopDbContext _context;
        private ProductHistoryRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ShopDbContext(options);
            _repository = new ProductHistoryRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void GetAll_ShouldReturnAllHistories()
        {
            _context.ProductHistories.Add(new ProductHistory { Id = 1, ProductId = 10, AddedQuantity = 5, Comment = "Новий завіз" });
            _context.ProductHistories.Add(new ProductHistory { Id = 2, ProductId = 10, AddedQuantity = 10, Comment = "Поповнення" });
            _context.SaveChanges();

            var result = _repository.GetAll();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetAll_ShouldReturnEmpty_WhenNoHistories()
        {
            var result = _repository.GetAll();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetById_ShouldReturnHistory_WhenExists()
        {
            _context.ProductHistories.Add(new ProductHistory { Id = 1, ProductId = 10, AddedQuantity = 15, Comment = "Прихід" });
            _context.SaveChanges();

            var result = _repository.GetById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AddedQuantity, Is.EqualTo(15));
            Assert.That(result.Comment, Is.EqualTo("Прихід"));
        }

        [Test]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            var result = _repository.GetById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetByProductId_ShouldReturnHistoriesForSpecificProduct()
        {
            _context.ProductHistories.Add(new ProductHistory { Id = 1, ProductId = 10, AddedQuantity = 5, Comment = "Партія A" });
            _context.ProductHistories.Add(new ProductHistory { Id = 2, ProductId = 20, AddedQuantity = 8, Comment = "Партія B" });
            _context.ProductHistories.Add(new ProductHistory { Id = 3, ProductId = 10, AddedQuantity = 12, Comment = "Партія C" });
            _context.SaveChanges();

            var result = _repository.GetByProductId(10);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(h => h.ProductId == 10), Is.True);
        }

        [Test]
        public void Add_ShouldAddHistoryToDb()
        {
            var history = new ProductHistory { Id = 1, ProductId = 10, AddedQuantity = 20, Comment = "Поставка" };

            _repository.Add(history);

            var saved = _context.ProductHistories.Find(1);
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.AddedQuantity, Is.EqualTo(20));
            Assert.That(saved.Comment, Is.EqualTo("Поставка"));
        }

        [Test]
        public void Add_ShouldThrowException_WhenNull()
        {
            Assert.That(() => _repository.Add(null!), Throws.Exception);
        }

        [Test]
        public void Update_ShouldUpdateHistoryInDb()
        {
            var history = new ProductHistory { Id = 1, ProductId = 10, AddedQuantity = 5, Comment = "Старий коментар" };
            _context.ProductHistories.Add(history);
            _context.SaveChanges();

            history.AddedQuantity = 10;
            history.Comment = "Оновлений коментар";
            _repository.Update(history);

            var updated = _context.ProductHistories.Find(1);
            Assert.That(updated!.AddedQuantity, Is.EqualTo(10));
            Assert.That(updated.Comment, Is.EqualTo("Оновлений коментар"));
        }

        [Test]
        public void Delete_ShouldRemoveHistoryFromDb()
        {
            var history = new ProductHistory { Id = 1, ProductId = 10, AddedQuantity = 5, Comment = "Тест" };
            _context.ProductHistories.Add(history);
            _context.SaveChanges();

            _repository.Delete(1);

            var deleted = _context.ProductHistories.Find(1);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Delete_ShouldNotFail_WhenIdNotExists()
        {
            Assert.That(() => _repository.Delete(999), Throws.Nothing);
        }
    }
}