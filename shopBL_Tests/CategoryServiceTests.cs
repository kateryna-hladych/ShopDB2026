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
        public void Constructor_NullRepositories_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CategoryService(null!, _products));
            Assert.Throws<ArgumentNullException>(() => new CategoryService(_categories, null!));
        }

        

        [Test]
        public void GetAllCategories_EmptyRepository_ReturnsEmptyCollection()
        {
            Assert.That(_service.GetAllCategories(), Is.Empty);
        }

        [Test]
        public void GetAllCategories_WithData_ReturnsAllCategories()
        {
            _service.AddCategory(new Category { Name = "Смартфони" });
            _service.AddCategory(new Category { Name = "Ноутбуки" });

            var result = _service.GetAllCategories().ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Name), Is.EquivalentTo(new[] { "Смартфони", "Ноутбуки" }));
        }

        

        [Test]
        public void AddCategory_ValidCategory_IsPassedToRepository()
        {
            _service.AddCategory(new Category { Name = "Смартфони" });

            Assert.That(_categories.AddCalls, Is.EqualTo(1));
            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Смартфони"));
        }

        [Test]
        public void AddCategory_ValidCategory_GetsGeneratedId()
        {
            var category = new Category { Name = "Смартфони" };

            _service.AddCategory(category);

            Assert.That(category.CategoryId, Is.GreaterThan(0));
        }

        [Test]
        public void AddCategory_NameWithSpaces_IsTrimmedBeforeSaving()
        {
            _service.AddCategory(new Category { Name = "   Смартфони  " });

            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Смартфони"));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        public void AddCategory_EmptyOrWhitespaceName_ThrowsBusinessRuleException(string name)
        {
            var ex = Assert.Throws<BusinessRuleException>(
                () => _service.AddCategory(new Category { Name = name }));

            Assert.That(ex!.Message, Does.Contain("порожньою"));
        }

        [Test]
        public void AddCategory_InvalidName_DoesNotCallRepository()
        {
            Assert.Throws<BusinessRuleException>(
                () => _service.AddCategory(new Category { Name = "  " }));

            Assert.That(_categories.AddCalls, Is.EqualTo(0));
        }

        [Test]
        public void AddCategory_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddCategory(null!));
        }

        

        [Test]
        public void UpdateCategory_ValidName_CallsRepositoryUpdate()
        {
            var category = new Category { Name = "Планшети" };
            _service.AddCategory(category);

            category.Name = "Планшети та електронні книги";
            _service.UpdateCategory(category);

            Assert.That(_categories.UpdateCalls, Is.EqualTo(1));
            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Планшети та електронні книги"));
        }

        [Test]
        public void UpdateCategory_NameWithSpaces_IsTrimmed()
        {
            var category = new Category { Name = "Планшети" };
            _service.AddCategory(category);

            category.Name = "  Ноутбуки  ";
            _service.UpdateCategory(category);

            Assert.That(category.Name, Is.EqualTo("Ноутбуки"));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void UpdateCategory_EmptyName_ThrowsAndDoesNotCallRepository(string name)
        {
            var category = new Category { Name = "Смартфони" };
            _service.AddCategory(category);

            category.Name = name;

            Assert.Throws<BusinessRuleException>(() => _service.UpdateCategory(category));
            Assert.That(_categories.UpdateCalls, Is.EqualTo(0));
        }

        

        [Test]
        public void DeleteCategory_ExistingCategoryWithoutProducts_RemovesIt()
        {
            var category = new Category { Name = "Смартфони" };
            _service.AddCategory(category);

            _service.DeleteCategory(category.CategoryId);

            Assert.That(_categories.Items, Is.Empty);
        }

        [Test]
        public void DeleteCategory_NonExistingId_DoesNothingAndDoesNotThrow()
        {
            _service.AddCategory(new Category { Name = "Смартфони" });

            Assert.DoesNotThrow(() => _service.DeleteCategory(9999));
            Assert.That(_categories.DeleteCalls, Is.EqualTo(0));
            Assert.That(_categories.Items, Has.Count.EqualTo(1));
        }

        [Test]
        public void DeleteCategory_CategoryWithProducts_ThrowsBusinessRuleException()
        {
            var category = new Category { Name = "Смартфони" };
            _service.AddCategory(category);
            _products.Add(new Product
            {
                Name = "Смартфон Samsung Galaxy S24",
                SellingPrice = 32999,
                StockQuantity = 10,
                CategoryId = category.CategoryId
            });

            var ex = Assert.Throws<BusinessRuleException>(
                () => _service.DeleteCategory(category.CategoryId));

            Assert.That(ex!.Message, Does.Contain("прив'язані товари"));
        }

        [Test]
        public void DeleteCategory_CategoryWithProducts_KeepsCategory()
        {
            var category = new Category { Name = "Ноутбуки" };
            _service.AddCategory(category);
            _products.Add(new Product
            {
                Name = "Ноутбук Lenovo IdeaPad 3",
                SellingPrice = 24999,
                StockQuantity = 5,
                CategoryId = category.CategoryId
            });

            Assert.Throws<BusinessRuleException>(
                () => _service.DeleteCategory(category.CategoryId));

            Assert.That(_categories.DeleteCalls, Is.EqualTo(0));
            Assert.That(_categories.Items, Has.Count.EqualTo(1));
        }

        [Test]
        public void DeleteCategory_ProductsOfAnotherCategory_DoNotBlockDeletion()
        {
            var first = new Category { Name = "Смартфони" };
            var second = new Category { Name = "Ноутбуки" };
            _service.AddCategory(first);
            _service.AddCategory(second);
            _products.Add(new Product
            {
                Name = "Ноутбук Lenovo IdeaPad 3",
                SellingPrice = 24999,
                StockQuantity = 5,
                CategoryId = second.CategoryId
            });

            Assert.DoesNotThrow(() => _service.DeleteCategory(first.CategoryId));
            Assert.That(_categories.Items.Single().Name, Is.EqualTo("Ноутбуки"));
        }
    }
}