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
            _categoryRepo.Add(_phones);
            _productRepo.Add(new Product
            {
                Name = "Смартфон Samsung Galaxy S23",
                CategoryId = _phones.CategoryId,
                PurchasePrice = 28000,
                SellingPrice = 34000,
                StockQuantity = 20
            });
            _historyRepo.Add(new ProductHistory
            {
                ProductId = 1,
                AddedQuantity = 20,
                Comment = "Початкове надходження",
                CreatedAt = DateTime.Now
            });
        }

        private MainViewModel Create(string role)
            => new MainViewModel(role, _productService, _categoryService, _historyService);

        // ---------- Конструктор ----------

        [Test]
        public void Constructor_NullServices_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new MainViewModel("admin", null!, _categoryService, _historyService));
            Assert.Throws<ArgumentNullException>(
                () => new MainViewModel("admin", _productService, null!, _historyService));
            Assert.Throws<ArgumentNullException>(
                () => new MainViewModel("admin", _productService, _categoryService, null!));
        }

        

        [TestCase("admin", true)]
        [TestCase("ADMIN", true)]
        [TestCase("Admin", true)]
        [TestCase("User", false)]
        [TestCase("user", false)]
        [TestCase("guest", false)]
        public void IsAdmin_DependsOnRole(string role, bool expected)
        {
            var vm = Create(role);

            Assert.That(vm.IsAdmin, Is.EqualTo(expected));
        }

        [Test]
        public void RoleDisplay_Admin_ShowsAdministrator()
        {
            Assert.That(Create("admin").RoleDisplay, Is.EqualTo("Адміністратор"));
        }

        [Test]
        public void RoleDisplay_RegularUser_ShowsUser()
        {
            Assert.That(Create("User").RoleDisplay, Is.EqualTo("Користувач"));
        }

        

        [Test]
        public void Constructor_LoadsCategoriesProductsAndHistories()
        {
            var vm = Create("admin");

            Assert.That(vm.Categories, Has.Count.EqualTo(1));
            Assert.That(vm.Products, Has.Count.EqualTo(1));
            Assert.That(vm.Histories, Has.Count.EqualTo(1));
        }

        [Test]
        public void Constructor_FillsProductCategoryFromCategories()
        {
            var vm = Create("admin");

            Assert.That(vm.Products.Single().Category.Name, Is.EqualTo("Смартфони"));
        }

        

        [Test]
        public void Admin_AddCommands_AreEnabled()
        {
            var vm = Create("admin");

            Assert.That(vm.AddProductCommand.CanExecute(null), Is.True);
            Assert.That(vm.AddCategoryCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Admin_DeleteAndEditCommands_AreDisabledWithoutSelection()
        {
            var vm = Create("admin");

            Assert.That(vm.DeleteProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.EditCategoryCommand.CanExecute(null), Is.False);
            Assert.That(vm.DeleteCategoryCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Admin_DeleteAndEditCommands_AreEnabledAfterSelection()
        {
            var vm = Create("admin");

            vm.SelectedProduct = vm.Products.First();
            vm.SelectedCategory = vm.Categories.First();

            Assert.That(vm.DeleteProductCommand.CanExecute(null), Is.True);
            Assert.That(vm.EditCategoryCommand.CanExecute(null), Is.True);
            Assert.That(vm.DeleteCategoryCommand.CanExecute(null), Is.True);
        }

        

        [Test]
        public void RegularUser_AllModifyingCommands_AreDisabled()
        {
            var vm = Create("User");

            Assert.That(vm.AddProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.AddCategoryCommand.CanExecute(null), Is.False);
            Assert.That(vm.DeleteProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.EditCategoryCommand.CanExecute(null), Is.False);
            Assert.That(vm.DeleteCategoryCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void RegularUser_CommandsStayDisabledEvenWithSelection()
        {
            var vm = Create("User");

            vm.SelectedProduct = vm.Products.First();
            vm.SelectedCategory = vm.Categories.First();

            Assert.That(vm.DeleteProductCommand.CanExecute(null), Is.False);
            Assert.That(vm.EditCategoryCommand.CanExecute(null), Is.False);
            Assert.That(vm.DeleteCategoryCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void RegularUser_CanStillViewData()
        {
            var vm = Create("User");

            Assert.That(vm.Products, Is.Not.Empty);
            Assert.That(vm.Categories, Is.Not.Empty);
            Assert.That(vm.Histories, Is.Not.Empty);
        }

        

        [Test]
        public void SelectedCategory_FillsCategoryNameInput()
        {
            var vm = Create("admin");

            vm.SelectedCategory = vm.Categories.First();

            Assert.That(vm.CategoryNameInput, Is.EqualTo("Смартфони"));
        }

        [Test]
        public void AddCategory_ValidName_AddsCategoryAndClearsInput()
        {
            var vm = Create("admin");

            vm.CategoryNameInput = "Планшети";
            vm.AddCategoryCommand.Execute(null);

            Assert.That(vm.Categories.Select(c => c.Name), Does.Contain("Планшети"));
            Assert.That(_categoryRepo.Items, Has.Count.EqualTo(2));
            Assert.That(vm.CategoryNameInput, Is.Empty);
        }

        [Test]
        public void DeleteCategory_WithoutProducts_RemovesItAndClearsSelection()
        {
            var tablets = new Category { Name = "Планшети" };
            _categoryRepo.Add(tablets);
            var vm = Create("admin");

            vm.SelectedCategory = vm.Categories.Single(c => c.Name == "Планшети");
            vm.DeleteCategoryCommand.Execute(null);

            Assert.That(vm.Categories.Select(c => c.Name), Does.Not.Contain("Планшети"));
            Assert.That(vm.SelectedCategory, Is.Null);
            Assert.That(vm.CategoryNameInput, Is.Empty);
        }

        

        [Test]
        public void AddProduct_ValidData_AddsProductWithChosenCategoryAndPrices()
        {
            var vm = Create("admin");

            vm.ProductNameInput = "Смартфон Apple iPhone 15 Pro";
            vm.ProductCategoryIdInput = _phones.CategoryId;
            vm.ProductPurchasePriceInput = 45000;
            vm.ProductPriceInput = 53000;
            vm.ProductStockInput = 12;
            vm.AddProductCommand.Execute(null);

            var saved = _productRepo.Items.Single(p => p.Name == "Смартфон Apple iPhone 15 Pro");
            Assert.That(saved.CategoryId, Is.EqualTo(_phones.CategoryId));
            Assert.That(saved.PurchasePrice, Is.EqualTo(45000m));
            Assert.That(saved.SellingPrice, Is.EqualTo(53000m));
            Assert.That(saved.StockQuantity, Is.EqualTo(12));
            Assert.That(vm.Products, Has.Count.EqualTo(2));
        }

        [Test]
        public void AddProduct_Success_ResetsInputFields()
        {
            var vm = Create("admin");

            vm.ProductNameInput = "Смартфон Apple iPhone 15 Pro";
            vm.ProductCategoryIdInput = _phones.CategoryId;
            vm.ProductPurchasePriceInput = 45000;
            vm.ProductPriceInput = 53000;
            vm.ProductStockInput = 12;
            vm.ProductIsBlockedInput = true;
            vm.AddProductCommand.Execute(null);

            Assert.That(vm.ProductNameInput, Is.Empty);
            Assert.That(vm.ProductCategoryIdInput, Is.Null);
            Assert.That(vm.ProductPurchasePriceInput, Is.EqualTo(0m));
            Assert.That(vm.ProductPriceInput, Is.EqualTo(0m));
            Assert.That(vm.ProductStockInput, Is.EqualTo(0));
            Assert.That(vm.ProductIsBlockedInput, Is.False);
        }

        [Test]
        public void AddProduct_BlockedFlag_IsSaved()
        {
            var vm = Create("admin");

            vm.ProductNameInput = "Знятий з продажу товар";
            vm.ProductCategoryIdInput = _phones.CategoryId;
            vm.ProductPriceInput = 1000;
            vm.ProductStockInput = 1;
            vm.ProductIsBlockedInput = true;
            vm.AddProductCommand.Execute(null);

            Assert.That(_productRepo.Items.Single(p => p.Name == "Знятий з продажу товар").IsBlocked, Is.True);
        }

        [Test]
        public void DeleteProduct_RemovesItFromListAndClearsSelection()
        {
            var vm = Create("admin");

            vm.SelectedProduct = vm.Products.First();
            vm.DeleteProductCommand.Execute(null);

            Assert.That(vm.Products, Is.Empty);
            Assert.That(vm.SelectedProduct, Is.Null);
        }

        [Test]
        public void DeleteProduct_AlsoRemovesItsHistoryFromList()
        {
            var vm = Create("admin");

            vm.SelectedProduct = vm.Products.First();
            vm.DeleteProductCommand.Execute(null);

            Assert.That(vm.Histories, Is.Empty);
        }

        

        [Test]
        public void LogoutCommand_IsAlwaysEnabled()
        {
            Assert.That(Create("admin").LogoutCommand.CanExecute(null), Is.True);
            Assert.That(Create("User").LogoutCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void LogoutCommand_RaisesLogoutRequested()
        {
            var vm = Create("User");
            bool raised = false;
            vm.LogoutRequested += (_, _) => raised = true;

            vm.LogoutCommand.Execute(null);

            Assert.That(raised, Is.True);
        }

        [Test]
        public void SelectedProduct_WhenChanged_RaisesPropertyChanged()
        {
            var vm = Create("admin");
            string? changed = null;
            vm.PropertyChanged += (_, e) => changed = e.PropertyName;

            vm.SelectedProduct = vm.Products.First();

            Assert.That(changed, Is.EqualTo(nameof(MainViewModel.SelectedProduct)));
        }
    }
}