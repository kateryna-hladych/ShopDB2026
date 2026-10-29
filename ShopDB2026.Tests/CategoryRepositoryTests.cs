using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ShopDB.Tests
{
    [TestFixture]
    public class CategoryRepositoryTests
    {
        private ShopDbContext _context = null!;
        private CategoryRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ShopDbContext(options);
            _repository = new CategoryRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void GetAll_ShouldReturnAllCategories()
        {
            _context.Categories.Add(new Category { CategoryId = 1, Name = "Категорія 1" });
            _context.Categories.Add(new Category { CategoryId = 2, Name = "Категорія 2" });
            _context.SaveChanges();

            var result = _repository.GetAll();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetById_ShouldReturnCategory_WhenExists()
        {
            _context.Categories.Add(new Category { CategoryId = 1, Name = "Телефони" });
            _context.SaveChanges();

            var result = _repository.GetById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Телефони"));
        }

        [Test]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            var result = _repository.GetById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Add_ShouldAddCategoryToDb()
        {
            var category = new Category { CategoryId = 1, Name = "Ноутбуки" };

            _repository.Add(category);

            var saved = _context.Categories.Find(1);
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Name, Is.EqualTo("Ноутбуки"));
        }

        [Test]
        public void Add_ShouldThrowException_WhenNull()
        {
            Assert.That(() => _repository.Add(null!), Throws.Exception);
        }

        [Test]
        public void Update_ShouldUpdateCategoryInDb()
        {
            var category = new Category { CategoryId = 1, Name = "Стара категорія" };
            _context.Categories.Add(category);
            _context.SaveChanges();

            category.Name = "Нова категорія";
            _repository.Update(category);

            var updated = _context.Categories.Find(1);
            Assert.That(updated!.Name, Is.EqualTo("Нова категорія"));
        }

        [Test]
        public void Delete_ShouldRemoveCategoryFromDb()
        {
            var category = new Category { CategoryId = 1, Name = "Тестова категорія" };
            _context.Categories.Add(category);
            _context.SaveChanges();

            _repository.Delete(1);

            var deleted = _context.Categories.Find(1);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Delete_ShouldNotFail_WhenIdNotExists()
        {
            Assert.That(() => _repository.Delete(999), Throws.Nothing);
        }
    }
}