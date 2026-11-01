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

        private Product MakeProduct() => new Product
        {
            Name = "Смартфон Samsung Galaxy S23",
            CategoryId = _category.CategoryId,
            PurchasePrice = 28000,
            SellingPrice = 34000,
            StockQuantity = 20
        };

        

        [Test]
        public void AddProduct_ValidProduct_IsSavedInChosenCategory()
        {
            _service.AddProduct(MakeProduct());

            var saved = _products.Items.Single();
            Assert.That(saved.CategoryId, Is.EqualTo(_category.CategoryId));
            Assert.That(saved.PurchasePrice, Is.EqualTo(28000m));
            Assert.That(saved.SellingPrice, Is.EqualTo(34000m));
        }

        [Test]
        public void AddProduct_EmptyName_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.Name = "   ";

            Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));
            Assert.That(_products.AddCalls, Is.EqualTo(0));
        }

        [Test]
        public void AddProduct_NonPositiveSellingPrice_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.SellingPrice = 0;

            Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));
        }

        [Test]
        public void AddProduct_NonPositivePurchasePrice_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.PurchasePrice = 0;

            Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));
        }

        [Test]
        public void AddProduct_UnknownCategory_ThrowsBusinessRuleException()
        {
            var product = MakeProduct();
            product.CategoryId = 9999;

            Assert.Throws<BusinessRuleException>(() => _service.AddProduct(product));
        }

        

        [Test]
        public void UpdateProduct_ChangesNameAndBothPrices()
        {
            var product = MakeProduct();
            _service.AddProduct(product);

            _service.UpdateProduct(product.ProductId, "Смартфон Samsung Galaxy S24", 30000, 36000);

            var saved = _products.Items.Single();
            Assert.That(saved.Name, Is.EqualTo("Смартфон Samsung Galaxy S24"));
            Assert.That(saved.PurchasePrice, Is.EqualTo(30000m));
            Assert.That(saved.SellingPrice, Is.EqualTo(36000m));
        }

        [Test]
        public void UpdateProduct_InvalidPrice_ThrowsAndKeepsOldValues()
        {
            var product = MakeProduct();
            _service.AddProduct(product);

            Assert.Throws<BusinessRuleException>(
                () => _service.UpdateProduct(product.ProductId, "Товар", 100, 0));

            Assert.That(_products.Items.Single().SellingPrice, Is.EqualTo(34000m));
            Assert.That(_products.UpdateCalls, Is.EqualTo(0));
        }

        

        [Test]
        public void SetProductBlocked_True_BlocksProduct()
        {
            var product = MakeProduct();
            _service.AddProduct(product);

            _service.SetProductBlocked(product.ProductId, true);

            Assert.That(_products.Items.Single().IsBlocked, Is.True);
        }

        [Test]
        public void SetProductBlocked_False_UnblocksProduct()
        {
            var product = MakeProduct();
            _service.AddProduct(product);
            _service.SetProductBlocked(product.ProductId, true);

            _service.SetProductBlocked(product.ProductId, false);

            Assert.That(_products.Items.Single().IsBlocked, Is.False);
        }

        [Test]
        public void SetProductBlocked_UnknownProduct_ThrowsBusinessRuleException()
        {
            Assert.Throws<BusinessRuleException>(() => _service.SetProductBlocked(9999, true));
        }

        

        [Test]
        public void DeleteProduct_RemovesProductAndItsHistory()
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

            _service.DeleteProduct(product.ProductId);

            Assert.That(_products.Items, Is.Empty);
            Assert.That(_histories.Items, Is.Empty);
        }
    }
}