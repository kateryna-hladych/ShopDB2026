using ShopBL;
using shopViewModels;
using ShopDB.DAL.EF.Entities;

namespace shopBL_Tests
{
    [TestFixture]
    public class MainViewModelTests
    {
        private FakeCategoryRepository _categoryRepo = null!;
        private FakeProductRepository _productRepo = null!;
        private FakeProductHistoryRepository _historyRepo = null!;
        private CategoryService _categoryService = null!;
        private ProductService _productService = null!;
        private ProductHistoryService _historyService = null!;
        private Category _phones = null!;
        private Category _laptops = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryRepo = new FakeCategoryRepository();
            _productRepo = new FakeProductRepository();
            _historyRepo = new FakeProductHistoryRepository();

            _categoryService = new CategoryService(_categoryRepo, _productRepo);
            _productService = new ProductService(_productRepo, _historyRepo, _categoryRepo);
            _historyService = new ProductHistoryService(_historyRepo);

            _phones = new Category { Name = "Смартфони" };
            _laptops = new Category { Name = "Ноутбуки" };
            _categoryRepo.Add(_phones);
            _categoryRepo.Add(_laptops);

            AddProduct("Смартфон Samsung Galaxy S23", _phones, 28000, 34000, 20);
            AddProduct("Ноутбук Lenovo IdeaPad 3", _laptops, 18000, 22500, 15);
            AddProduct("Смартфон Apple iPhone 15 Pro", _phones, 45000, 53000, 12);
        }

        private void AddProduct(string name, Category category, decimal purchase, decimal selling, int stock)
        {
            _productRepo.Add(new Product
            {
                Name = name,
                CategoryId = category.CategoryId,
                PurchasePrice = purchase,
                SellingPrice = selling,
                StockQuantity = stock
            });
        }

        private MainViewModel Create(string role)
            => new MainViewModel(role, _productService, _categoryService, _historyService);

        

        [Test]
        public void ProductManager_CanAddAndEditAfterSelectingProduct()
        {
            var vm = Create("ProductManager");

            Assert.That(vm.RoleDisplay, Is.EqualTo("Менеджер товарів"));
            Assert.That(vm.AddProductCommand.CanExecute(null), Is.True);
            Assert.That(vm.AddCategoryCommand.CanExecute(null), Is.True);
            Assert.That(vm.EditProductCommand.CanExecute(null), Is.False);

            vm.SelectedProduct = vm.Products.First();

            Assert.That(vm.EditProductCommand.CanExecute(null), Is.True);
            Assert.That(vm.ToggleBlockProductCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void RegularUser_CannotModifyAnything()
        {
            var vm = Create("User");
            vm.SelectedProduct = vm.Products.First();
            vm.SelectedCategory = vm.Categories.First();

            Assert.That(vm.AddProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.EditProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.ToggleBlockProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.DeleteProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.AddCategoryCommand.CanExecute(null), Is.False);
            Assert.That(vm.EditCategoryCommand.CanExecute(null), Is.False);
            Assert.That(vm.DeleteCategoryCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void RegularUser_CanStillViewData()
        {
            var vm = Create("User");

            Assert.That(vm.Products, Is.Not.Empty);
            Assert.That(vm.Categories, Is.Not.Empty);
        }

        

        [Test]
        public void SelectingProduct_FillsFormWithItsData()
        {
            var vm = Create("ProductManager");

            vm.SelectedProduct = vm.Products.Single(p => p.Name.Contains("Samsung"));

            Assert.That(vm.ProductNameInput, Is.EqualTo("Смартфон Samsung Galaxy S23"));
            Assert.That(vm.ProductCategoryIdInput, Is.EqualTo(_phones.CategoryId));
            Assert.That(vm.ProductPurchasePriceInput, Is.EqualTo(28000m));
            Assert.That(vm.ProductPriceInput, Is.EqualTo(34000m));
        }

        [Test]
        public void EditProduct_ChangesPurchaseAndSellingPrice()
        {
            var vm = Create("ProductManager");
            vm.SelectedProduct = vm.Products.Single(p => p.Name.Contains("Samsung"));

            vm.ProductPurchasePriceInput = 29000;
            vm.ProductPriceInput = 36000;
            vm.EditProductCommand.Execute(null);

            var saved = _productRepo.Items.Single(p => p.Name.Contains("Samsung"));
            Assert.That(saved.PurchasePrice, Is.EqualTo(29000m));
            Assert.That(saved.SellingPrice, Is.EqualTo(36000m));
        }

        [Test]
        public void ToggleBlock_BlocksAndThenUnblocksSelectedProduct()
        {
            var vm = Create("ProductManager");
            vm.SelectedProduct = vm.Products.Single(p => p.Name.Contains("Samsung"));

            vm.ToggleBlockProductCommand.Execute(null);
            Assert.That(_productRepo.Items.Single(p => p.Name.Contains("Samsung")).IsBlocked, Is.True);

            vm.ToggleBlockProductCommand.Execute(null);
            Assert.That(_productRepo.Items.Single(p => p.Name.Contains("Samsung")).IsBlocked, Is.False);
        }

        [Test]
        public void AddProduct_SavesProductInChosenCategory()
        {
            var vm = Create("ProductManager");

            vm.ProductNameInput = "Ноутбук ASUS ROG Strix";
            vm.ProductCategoryIdInput = _laptops.CategoryId;
            vm.ProductPurchasePriceInput = 42000;
            vm.ProductPriceInput = 51000;
            vm.ProductStockInput = 8;
            vm.AddProductCommand.Execute(null);

            var saved = _productRepo.Items.Single(p => p.Name == "Ноутбук ASUS ROG Strix");
            Assert.That(saved.CategoryId, Is.EqualTo(_laptops.CategoryId));
            Assert.That(saved.PurchasePrice, Is.EqualTo(42000m));
        }

        [Test]
        public void AddCategory_AddsCategoryToList()
        {
            var vm = Create("ProductManager");

            vm.CategoryNameInput = "Планшети";
            vm.AddCategoryCommand.Execute(null);

            Assert.That(vm.Categories.Select(c => c.Name), Does.Contain("Планшети"));
        }

        

        [Test]
        public void Search_FiltersProductsByPartOfName()
        {
            var vm = Create("User");

            vm.SearchText = "смартфон";

            Assert.That(vm.Products, Has.Count.EqualTo(2));
            Assert.That(vm.Products.Select(p => p.Name), Has.All.Contain("Смартфон"));
        }

        [Test]
        public void Sort_BySellingPrice_Ascending()
        {
            var vm = Create("User");

            vm.SelectedSortOption = "Ціна продажу";

            Assert.That(vm.Products.Select(p => p.SellingPrice), Is.EqualTo(new[] { 22500m, 34000m, 53000m }));
        }

        [Test]
        public void Sort_BySellingPrice_Descending()
        {
            var vm = Create("User");

            vm.SelectedSortOption = "Ціна продажу";
            vm.SortDescending = true;

            Assert.That(vm.Products.Select(p => p.SellingPrice), Is.EqualTo(new[] { 53000m, 34000m, 22500m }));
        }

        

        [Test]
        public void Logout_RaisesLogoutRequested()
        {
            var vm = Create("ProductManager");
            bool raised = false;
            vm.LogoutRequested += (_, _) => raised = true;

            vm.LogoutCommand.Execute(null);

            Assert.That(raised, Is.True);
        }
    }
}