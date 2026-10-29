using ShopBL;
using ShopDB.DAL.EF.Entities;

namespace shopBL_Tests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private FakeProductRepository _products = null!;
        private FakeProductHistoryRepository _histories = null!;
        private FakeCategoryRepository _categories = null!;
        private ProductService _service = null!;
        private Category _category = null!;

        [SetUp]
        public void SetUp()
        {
            _products = new FakeProductRepository();
            _histories = new FakeProductHistoryRepository();
            _categories = new FakeCategoryRepository();
            _service = new ProductService(_products, _histories, _categories);

            _category = new Category { Name = "Смартфони" };
            _categories.Add(_category);
        }

        private Product MakeProduct(string name = "Смартфон Samsung Galaxy S23") => new Product
        {
            Name = name,
            CategoryId = _category.CategoryId,
            PurchasePrice = 28000,
            SellingPrice = 34000,
            StockQuantity = 20
        };

        [Test]
        public void Constructor_NullRepositories_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductService(null!, _histories, _categories));
            Assert.Throws<ArgumentNullException>(() => new ProductService(_products, null!, _categories));
            Assert.Throws<ArgumentNullException>(() => new ProductService(_products, _histories, null!));
        }

        

        [Test]
        public void GetAllProducts_EmptyRepository_ReturnsEmptyCollection()
        {
            Assert.That(_service.GetAllProducts(), Is.Empty);
        }

        [Test]
        public void GetAllProducts_WithData_ReturnsAllProducts()
        {
            _service.AddProduct(MakeProduct("Смартфон Samsung Galaxy S23"));
            _service.AddProduct(MakeProduct("Смартфон Apple iPhone 15 Pro"));

            Assert.That(_service.GetAllProducts().Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetAllCategories_ReturnsCategoriesFromRepository()
        {
            var result = _service.GetAllCategories().ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Смартфони"));
        }

        

        [Test]
        public void AddProduct_ValidProduct_IsPassedToRepository()
        {
            _service.AddProduct(MakeProduct());

            Assert.That(_products.AddCalls, Is.EqualTo(1));
            var saved = _products.Items.Single();
            Assert.That(saved.Name, Is.EqualTo("Смартфон Samsung Galaxy S23"));
            Assert.That(saved.PurchasePrice, Is.EqualTo(28000m));
            Assert.That(saved.SellingPrice, Is.EqualTo(34000m));
            Assert.That(saved.StockQuantity, Is.EqualTo(20));
        }

        [Test]
        public void AddProduct_ValidProduct_GetsGeneratedId()
        {
            var product = MakeProduct();

            _service.AddProduct(product);

            Assert.That(product.ProductId, Is.GreaterThan(0));
        }

        [Test]
        public void AddProduct_NameWithSpaces_IsTrimmed()
        {
            _service.AddProduct(MakeProduct("   Монітор 27\" Dell UltraSharp  "));

            Assert.That(_products.Items.Single().Name, Is.EqualTo("Монітор 27\" Dell UltraSharp"));
        }

        [Test]
        public void AddProduct_ZeroStock_IsAllowed()
        {
            var product = MakeProduct();
            product.StockQuantity = 0;

            Assert.DoesNotThrow(() => _service.AddProduct(product));
        }

        [Test]
        public void AddProduct_ZeroPurchasePrice_IsAllowed()
        {
            var product = MakeProduct();
            product.PurchasePrice = 0;

            Assert.DoesNotThrow(() => _service.AddProduct(product));
        }

        

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        public void AddProduct_EmptyName_ThrowsBusinessRuleException(string name)
        {
            var ex = Assert.Throws<BusinessRuleException>(
                () => _service.AddProduct(MakeProduct(name)));

            Assert.That(ex!.Message, Does.Contain("Назва товару"));
        }

        [Test]
        public void AddProduct_NegativePurchasePrice_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.PurchasePrice = -1;

            var ex = Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));

            Assert.That(ex!.Message, Does.Contain("Закупівельна"));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void AddProduct_NonPositiveSellingPrice_ThrowsBusinessRuleException(int price)
        {
            var product = MakeProduct();
            product.SellingPrice = price;

            var ex = Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));

            Assert.That(ex!.Message, Does.Contain("Ціна продажу"));
        }

        [Test]
        public void AddProduct_NegativeStock_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.StockQuantity = -1;

            var ex = Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));

            Assert.That(ex!.Message, Does.Contain("Кількість"));
        }

        [Test]
        public void AddProduct_UnknownCategory_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.CategoryId = 9999;

            var ex = Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));

            Assert.That(ex!.Message, Does.Contain("Категорію"));
        }

        [Test]
        public void AddProduct_InvalidProduct_DoesNotCallRepository()
        {
            var product = MakeProduct();
            product.SellingPrice = 0;

            Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));

            Assert.That(_products.AddCalls, Is.EqualTo(0));
        }

        [Test]
        public void AddProduct_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddProduct(null!));
        }

        

        [Test]
        public void DeleteProduct_ExistingProduct_RemovesIt()
        {
            var product = MakeProduct();
            _service.AddProduct(product);

            _service.DeleteProduct(product.ProductId);

            Assert.That(_products.Items, Is.Empty);
        }

        [Test]
        public void DeleteProduct_RemovesItsHistoryRecords()
        {
            var product = MakeProduct();
            _service.AddProduct(product);
            _histories.Add(new ProductHistory
            {
                ProductId = product.ProductId,
                AddedQuantity = 5,
                Comment = "Надходження",
                CreatedAt = DateTime.Now
            });
            _histories.Add(new ProductHistory
            {
                ProductId = product.ProductId,
                AddedQuantity = 3,
                Comment = "Ще одне надходження",
                CreatedAt = DateTime.Now
            });

            _service.DeleteProduct(product.ProductId);

            Assert.That(_histories.Items, Is.Empty);
        }

        [Test]
        public void DeleteProduct_KeepsHistoryOfOtherProducts()
        {
            var first = MakeProduct("Смартфон Samsung Galaxy S23");
            var second = MakeProduct("Смартфон Apple iPhone 15 Pro");
            _service.AddProduct(first);
            _service.AddProduct(second);
            _histories.Add(new ProductHistory
            {
                ProductId = first.ProductId,
                AddedQuantity = 5,
                Comment = "Перший товар",
                CreatedAt = DateTime.Now
            });
            _histories.Add(new ProductHistory
            {
                ProductId = second.ProductId,
                AddedQuantity = 7,
                Comment = "Другий товар",
                CreatedAt = DateTime.Now
            });

            _service.DeleteProduct(first.ProductId);

            Assert.That(_histories.Items, Has.Count.EqualTo(1));
            Assert.That(_histories.Items.Single().ProductId, Is.EqualTo(second.ProductId));
        }

        [Test]
        public void DeleteProduct_OnlyDeletesRequestedProduct()
        {
            var first = MakeProduct("Смартфон Samsung Galaxy S23");
            var second = MakeProduct("Смартфон Apple iPhone 15 Pro");
            _service.AddProduct(first);
            _service.AddProduct(second);

            _service.DeleteProduct(first.ProductId);

            Assert.That(_products.Items.Single().Name, Is.EqualTo("Смартфон Apple iPhone 15 Pro"));
        }

        [Test]
        public void DeleteProduct_NonExistingId_DoesNothingAndDoesNotThrow()
        {
            _service.AddProduct(MakeProduct());

            Assert.DoesNotThrow(() => _service.DeleteProduct(9999));
            Assert.That(_products.DeleteCalls, Is.EqualTo(0));
            Assert.That(_products.Items, Has.Count.EqualTo(1));
        }
    }
}