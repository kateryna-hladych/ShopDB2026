using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ShopBL;
using ShopDB.DAL.EF.Entities;

namespace shopViewModels
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private const string ManagerRole = "ProductManager";

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductHistoryService _historyService;

        
        private readonly List<Product> _allProducts = new();

        public string UserRole { get; set; }

        public ObservableCollection<Product> Products { get; set; } = new();
        public ObservableCollection<Category> Categories { get; set; } = new();
        public ObservableCollection<ProductHistory> Histories { get; set; } = new();

        
        public bool IsAdmin => string.Equals(UserRole, "admin", StringComparison.OrdinalIgnoreCase);
        public bool IsManager => string.Equals(UserRole, ManagerRole, StringComparison.OrdinalIgnoreCase);

        
        public bool CanManage => IsAdmin || IsManager;

        public string RoleDisplay => IsAdmin ? "Адміністратор" : IsManager ? "Менеджер товарів" : "Користувач";

        
        public string[] SortOptions { get; } =
            { "ID", "Назва", "Категорія", "Закупівельна ціна", "Ціна продажу", "Кількість" };

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                OnPropertyChanged(nameof(SearchText));
                ApplyProductView();
            }
        }

        private string _selectedSortOption = "ID";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value ?? "ID";
                OnPropertyChanged(nameof(SelectedSortOption));
                ApplyProductView();
            }
        }

        private bool _sortDescending;
        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                _sortDescending = value;
                OnPropertyChanged(nameof(SortDescending));
                ApplyProductView();
            }
        }

        
        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                
                if (value != null)
                {
                    ProductNameInput = value.Name;
                    ProductCategoryIdInput = value.CategoryId;
                    ProductPurchasePriceInput = value.PurchasePrice;
                    ProductPriceInput = value.SellingPrice;
                    ProductStockInput = value.StockQuantity;
                    ProductIsBlockedInput = value.IsBlocked;
                }

                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _productNameInput = string.Empty;
        public string ProductNameInput
        {
            get => _productNameInput;
            set { _productNameInput = value; OnPropertyChanged(nameof(ProductNameInput)); }
        }

        private decimal _productPurchasePriceInput;
        public decimal ProductPurchasePriceInput
        {
            get => _productPurchasePriceInput;
            set { _productPurchasePriceInput = value; OnPropertyChanged(nameof(ProductPurchasePriceInput)); }
        }

        private decimal _productPriceInput;
        public decimal ProductPriceInput
        {
            get => _productPriceInput;
            set { _productPriceInput = value; OnPropertyChanged(nameof(ProductPriceInput)); }
        }

        private int _productStockInput;
        public int ProductStockInput
        {
            get => _productStockInput;
            set { _productStockInput = value; OnPropertyChanged(nameof(ProductStockInput)); }
        }

        private int? _productCategoryIdInput;
        public int? ProductCategoryIdInput
        {
            get => _productCategoryIdInput;
            set { _productCategoryIdInput = value; OnPropertyChanged(nameof(ProductCategoryIdInput)); }
        }

        private bool _productIsBlockedInput;
        public bool ProductIsBlockedInput
        {
            get => _productIsBlockedInput;
            set { _productIsBlockedInput = value; OnPropertyChanged(nameof(ProductIsBlockedInput)); }
        }

        
        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                if (_selectedCategory != null)
                {
                    CategoryNameInput = _selectedCategory.Name; 
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _categoryNameInput = string.Empty;
        public string CategoryNameInput
        {
            get => _categoryNameInput;
            set { _categoryNameInput = value; OnPropertyChanged(nameof(CategoryNameInput)); }
        }

        
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand ToggleBlockProductCommand { get; }
        public ICommand ClearProductFormCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public ICommand LogoutCommand { get; }

        
        public event EventHandler? LogoutRequested;

        
        public MainViewModel(string role, IProductService productService, ICategoryService categoryService, IProductHistoryService historyService)
        {
            UserRole = role;
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));

            
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => CanManage);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => CanManage && SelectedProduct != null);
            ToggleBlockProductCommand = new RelayCommand(_ => ToggleBlockProduct(), _ => CanManage && SelectedProduct != null);
            ClearProductFormCommand = new RelayCommand(_ => ClearProductForm());
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => CanManage && SelectedProduct != null);

            AddCategoryCommand = new RelayCommand(_ => AddCategory(), _ => CanManage);
            EditCategoryCommand = new RelayCommand(_ => EditCategory(), _ => CanManage && SelectedCategory != null);
            DeleteCategoryCommand = new RelayCommand(_ => DeleteCategory(), _ => CanManage && SelectedCategory != null);

            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke(this, EventArgs.Empty));

            LoadData();
        }

        private void LoadData()
        {
            
            LoadCategories();
            LoadProducts();
            LoadHistories();
        }

        private void LoadProducts()
        {
            try
            {
                var list = _productService.GetAllProducts();
                _allProducts.Clear();
                foreach (var item in list)
                {
                    
                    if (item.Category == null)
                    {
                        item.Category = Categories.FirstOrDefault(c => c.CategoryId == item.CategoryId)!;
                    }
                    _allProducts.Add(item);
                }
                ApplyProductView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження товарів: {ex.Message}");
            }
        }

        
        private void ApplyProductView()
        {
            var selectedId = _selectedProduct?.ProductId;

            IEnumerable<Product> query = _allProducts;

            var text = (SearchText ?? string.Empty).Trim();
            if (text.Length > 0)
            {
                query = query.Where(p => (p.Name ?? string.Empty)
                    .Contains(text, StringComparison.CurrentCultureIgnoreCase));
            }

            query = SelectedSortOption switch
            {
                "Назва" => Order(query, p => p.Name),
                "Категорія" => Order(query, p => p.Category?.Name ?? string.Empty),
                "Закупівельна ціна" => Order(query, p => p.PurchasePrice),
                "Ціна продажу" => Order(query, p => p.SellingPrice),
                "Кількість" => Order(query, p => p.StockQuantity),
                _ => Order(query, p => p.ProductId)
            };

            Products.Clear();
            foreach (var product in query) Products.Add(product);

            
            _selectedProduct = selectedId == null
                ? null
                : Products.FirstOrDefault(p => p.ProductId == selectedId);
            OnPropertyChanged(nameof(SelectedProduct));
            CommandManager.InvalidateRequerySuggested();
        }

        private IEnumerable<Product> Order<TKey>(IEnumerable<Product> source, Func<Product, TKey> key)
            => SortDescending ? source.OrderByDescending(key) : source.OrderBy(key);

        private void LoadCategories()
        {
            try
            {
                var list = _categoryService.GetAllCategories();
                Categories.Clear();
                foreach (var item in list) Categories.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження категорій: {ex.Message}");
            }
        }

        private void LoadHistories()
        {
            try
            {
                var list = _historyService.GetAllHistories();
                Histories.Clear();
                foreach (var item in list) Histories.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії: {ex.Message}");
            }
        }

        
        private void AddProduct()
        {
            
            if (string.IsNullOrWhiteSpace(ProductNameInput))
            {
                MessageBox.Show("Введіть назву товару.", "Валідація");
                return;
            }
            if (ProductCategoryIdInput == null)
            {
                MessageBox.Show("Оберіть категорію товару.", "Валідація");
                return;
            }
            if (ProductPurchasePriceInput <= 0)
            {
                MessageBox.Show("Закупівельна ціна має бути більшою за 0.", "Валідація");
                return;
            }
            if (ProductPriceInput <= 0)
            {
                MessageBox.Show("Ціна продажу має бути більшою за 0.", "Валідація");
                return;
            }
            if (ProductStockInput < 0)
            {
                MessageBox.Show("Кількість не може бути від'ємною.", "Валідація");
                return;
            }

            try
            {
                var product = new Product
                {
                    Name = ProductNameInput.Trim(),
                    CategoryId = ProductCategoryIdInput.Value,
                    PurchasePrice = ProductPurchasePriceInput,
                    SellingPrice = ProductPriceInput,
                    StockQuantity = ProductStockInput,
                    IsBlocked = ProductIsBlockedInput
                };

                _productService.AddProduct(product);
                LoadData();

                ClearProductForm();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Помилка додавання: {errorMessage}");
            }
        }

        
        private void EditProduct()
        {
            if (SelectedProduct == null) return;

            try
            {
                int id = SelectedProduct.ProductId;

                _productService.UpdateProduct(id, ProductNameInput, ProductPurchasePriceInput, ProductPriceInput);
                LoadData();

                SelectedProduct = Products.FirstOrDefault(p => p.ProductId == id);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Помилка редагування товару: {errorMessage}");
            }
        }

        
        private void ToggleBlockProduct()
        {
            if (SelectedProduct == null) return;

            try
            {
                int id = SelectedProduct.ProductId;
                bool newState = !SelectedProduct.IsBlocked;

                _productService.SetProductBlocked(id, newState);
                LoadData();

                SelectedProduct = Products.FirstOrDefault(p => p.ProductId == id);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Помилка блокування товару: {errorMessage}");
            }
        }

        private void ClearProductForm()
        {
            SelectedProduct = null;
            ProductNameInput = string.Empty;
            ProductCategoryIdInput = null;
            ProductPurchasePriceInput = 0;
            ProductPriceInput = 0;
            ProductStockInput = 0;
            ProductIsBlockedInput = false;
        }

        private void DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                try
                {
                    _productService.DeleteProduct(SelectedProduct.ProductId);
                    LoadData();
                    SelectedProduct = null;
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"Помилка видалення: {errorMessage}");
                }
            }
        }

        
        private void AddCategory()
        {
            
            if (string.IsNullOrWhiteSpace(CategoryNameInput))
            {
                MessageBox.Show("Введіть назву категорії.", "Валідація");
                return;
            }

            try
            {
                var category = new Category { Name = CategoryNameInput };
                _categoryService.AddCategory(category);
                LoadData();
                CategoryNameInput = string.Empty;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Помилка додавання категорії: {errorMessage}");
            }
        }

        private void EditCategory()
        {
            
            if (string.IsNullOrWhiteSpace(CategoryNameInput))
            {
                MessageBox.Show("Введіть назву категорії.", "Валідація");
                return;
            }

            if (SelectedCategory != null)
            {
                try
                {
                    SelectedCategory.Name = CategoryNameInput;

                    
                    _categoryService.UpdateCategory(SelectedCategory);

                    LoadData(); 
                    MessageBox.Show("Категорію успішно оновлено!");
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"Помилка редагування: {errorMessage}");
                }
            }
        }

        private void DeleteCategory()
        {
            if (SelectedCategory != null)
            {
                try
                {
                    _categoryService.DeleteCategory(SelectedCategory.CategoryId);
                    LoadData();
                    SelectedCategory = null;
                    CategoryNameInput = string.Empty;
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"Помилка видалення: {errorMessage}");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}