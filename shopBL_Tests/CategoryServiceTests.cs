using ShopBL;
using ShopDB.DAL.EF.Entities;

namespace shopBL_Tests
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private FakeCategoryRepository _categories = null!;
        private FakeProductRepository _products = null!;
        private CategoryService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _categories = new FakeCategoryRepository();
            _products = new FakeProductRepository();
            _service = new CategoryService(_categories, _products);
        }

        [Test]
        public void AddCategory_ValidName_SavesCategory()
        {
            _service.AddCategory(new Category { Name = "Смартфони" });

            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Смартфони"));
        }

        [Test]
        public void AddCategory_NameWithSpaces_IsTrimmedBeforeSaving()
        {
            _service.AddCategory(new Category { Name = "   Смартфони  " });

            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Смартфони"));
        }

        [Test]
        public void AddCategory_EmptyName_ThrowsBusinessRuleException()
        {
            Assert.Throws<BusinessRuleException>(
                () => _service.AddCategory(new Category { Name = "   " }));

            Assert.That(_categories.AddCalls, Is.EqualTo(0));
        }

        [Test]
        public void UpdateCategory_ValidName_ChangesName()
        {
            var category = new Category { Name = "Планшети" };
            _service.AddCategory(category);

            category.Name = "Планшети та електронні книги";
            _service.UpdateCategory(category);

            Assert.That(_categories.UpdateCalls, Is.EqualTo(1));
            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Планшети та електронні книги"));
        }

        [Test]
        public void DeleteCategory_WithoutProducts_RemovesCategory()
        {
            var category = new Category { Name = "Смартфони" };
            _service.AddCategory(category);

            _service.DeleteCategory(category.CategoryId);

            Assert.That(_categories.Items, Is.Empty);
        }

        [Test]
        public void DeleteCategory_WithProducts_ThrowsAndKeepsCategory()
        {
            var category = new Category { Name = "Смартфони" };
            _service.AddCategory(category);
            _products.Add(new Product
            {
                Name = "Смартфон Samsung Galaxy S23",
                CategoryId = category.CategoryId,
                PurchasePrice = 28000,
                SellingPrice = 34000,
                StockQuantity = 20
            });

            Assert.Throws<BusinessRuleException>(
                () => _service.DeleteCategory(category.CategoryId));

            Assert.That(_categories.Items, Has.Count.EqualTo(1));
        }
    }
}