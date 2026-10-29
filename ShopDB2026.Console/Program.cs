using System.Text;
using ShopDB.DAL.EF.Data;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories;
using ShopDB.DAL.EF.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
namespace ShopDB.Console;

internal class Program
{
    static void Main(string[] args)
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

        var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ShopDbContext(optionsBuilder.Options);
        IProductRepository productRepo = new ProductRepository(context);
        ICategoryRepository categoryRepo = new CategoryRepository(context);

        while (true)
        {
            System.Console.WriteLine("УПРАВЛІННЯ ТОВАРАМИ:");
            System.Console.WriteLine("1. Показати всі активні товари");
            System.Console.WriteLine("2. Знайти товар за ID");
            System.Console.WriteLine("3. Додати новий товар");
            System.Console.WriteLine("4. Оновити товар");
            System.Console.WriteLine("5. Видалити товар");
            System.Console.WriteLine("0. Спочатку");
            System.Console.Write("цифра: ");

            string choice = System.Console.ReadLine() ?? "";
            System.Console.WriteLine();

            switch (choice)
            {
                case "1":
                    ShowAll(productRepo);
                    break;
                case "2":
                    ShowById(productRepo);
                    break;
                case "3":
                    Add(productRepo, categoryRepo);
                    break;
                case "4":
                    Update(productRepo, categoryRepo);
                    break;
                case "5":
                    Delete(productRepo);
                    break;
                case "0":
                    System.Console.WriteLine("\n press клавішу для виходу");
                    System.Console.ReadKey();
                    return;
                default:
                    System.Console.WriteLine("шось не те обрали..");
                    break;
            }
        }
    }

    private static void ShowAll(IProductRepository _productRepo)
    {
        var products = _productRepo.GetAll();

        System.Console.WriteLine("Список товарів:");
        foreach (var p in products)
        {
            System.Console.WriteLine($"ID: {p.ProductId} | Назва: {p.Name} | Ціна продажу: {p.SellingPrice} | Кількість: {p.StockQuantity}");
        }
    }

    private static void ShowById(IProductRepository _productRepo)
    {
        System.Console.Write("Введіть ID товару: ");
        int id = int.Parse(System.Console.ReadLine() ?? "0");

        var product = _productRepo.GetById(id);

        if (product != null)
        {
            System.Console.WriteLine($"ID: {product.ProductId} | Назва: {product.Name} | Ціна купівлі: {product.PurchasePrice} | Ціна продажу: {product.SellingPrice} | Залишок: {product.StockQuantity}");
        }
        else
        {
            System.Console.WriteLine("Товар не знайдено:(");
        }
    }

   private static void Add(IProductRepository _productRepo, ICategoryRepository _categoryRepo)
    {
        var product = new Product();

        System.Console.Write("Назва товару: ");
        product.Name = System.Console.ReadLine() ?? "";

        
        System.Console.WriteLine("\nДоступні категорії:");
        var categories = _categoryRepo.GetAll();
        if (!categories.Any())
        {
            System.Console.WriteLine(" немає жодної категорії!");
            return;
        }

        foreach (var cat in categories)
        {
            System.Console.WriteLine($"ID: {cat.CategoryId} | Назва: {cat.Name}");
        }
       

        
        System.Console.Write("Введіть ID категорії: ");
        int categoryId;
        while (!int.TryParse(System.Console.ReadLine(), out categoryId) || _categoryRepo.GetById(categoryId) == null)
        {
            System.Console.Write("Помилка! Такого ID категорії не існує або введено не число ");
        }
        product.CategoryId = categoryId;

       
        System.Console.Write("Ціна купівлі: ");
        decimal purchasePrice;
        while (!decimal.TryParse(System.Console.ReadLine(), out purchasePrice))
        {
            System.Console.Write("Помилка! Введіть правильну ціну (наприклад, 100): ");
        }
        product.PurchasePrice = purchasePrice;

       
        System.Console.Write("Ціна продажу: ");
        decimal sellingPrice;
        while (!decimal.TryParse(System.Console.ReadLine(), out sellingPrice))
        {
            System.Console.Write("Помилка! Введіть правильну ціну продажу: ");
        }
        product.SellingPrice = sellingPrice;

       
        System.Console.Write("Кількість на складі: ");
        int stockQuantity;
        while (!int.TryParse(System.Console.ReadLine(), out stockQuantity))
        {
            System.Console.Write("Помилка! Введіть ціле число для кількості: ");
        }
        product.StockQuantity = stockQuantity;

        
        try
        {
            _productRepo.Add(product);
            System.Console.WriteLine("Товар успішно додано! :)");
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Помилка при збереженні в базу: {ex.Message}");
        }
    }
   
    private static void Update(IProductRepository _productRepo, ICategoryRepository _categoryRepo)
    {
        System.Console.Write("Введіть ID товару для оновлення: ");
        int id;
        while (!int.TryParse(System.Console.ReadLine(), out id))
        {
            System.Console.Write("Помилка! Введіть ціле число для ID товару: ");
        }

        var product = _productRepo.GetById(id);
        if (product == null)
        {
            System.Console.WriteLine("Товар із таким ID не знайдено!");
            return;
        }

        System.Console.WriteLine($"Знайдено товар: {product.Name}. Залиште рядок пустим, якщо не хочете змінювати значення.");

        
        System.Console.Write($"Нова назва [{product.Name}]: ");
        string name = System.Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(name))
        {
            product.Name = name;
        }

        
        System.Console.WriteLine("\nДоступні категорії:");
        var categories = _categoryRepo.GetAll();
        foreach (var cat in categories)
        {
            System.Console.WriteLine($"ID: {cat.CategoryId} | Назва: {cat.Name}");
        }
      
        System.Console.Write($"Новий ID категорії [{product.CategoryId}] (натисніть Enter, щоб пропустити): ");
        string catInput = System.Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(catInput))
        {
            int categoryId;
            while (!int.TryParse(catInput, out categoryId) || _categoryRepo.GetById(categoryId) == null)
            {
                System.Console.Write("Помилка! Такого ID категорії не існує або введено не число. Спробуйте ще раз: ");
                catInput = System.Console.ReadLine() ?? "";
            }
            product.CategoryId = categoryId;
        }

        
        System.Console.Write($"Нова ціна продажу [{product.SellingPrice}]: ");
        string priceInput = System.Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(priceInput))
        {
            decimal sellingPrice;
            while (!decimal.TryParse(priceInput, out sellingPrice))
            {
                System.Console.Write("Помилка! Введіть правильну ціну продажу: ");
                priceInput = System.Console.ReadLine() ?? "";
            }
            product.SellingPrice = sellingPrice;
        }

        
        System.Console.Write($"Нова кількість на складі [{product.StockQuantity}]: ");
        string qtyInput = System.Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(qtyInput))
        {
            int stockQuantity;
            while (!int.TryParse(qtyInput, out stockQuantity))
            {
                System.Console.Write("Помилка! Введіть ціле число для кількості: ");
                qtyInput = System.Console.ReadLine() ?? "";
            }
            product.StockQuantity = stockQuantity;
        }

        try
        {
            _productRepo.Update(product);
            System.Console.WriteLine("Товар успішно оновлено! :)");
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Помилка при оновленні в базі: {ex.Message}");
        }
    }
    private static void Delete(IProductRepository _productRepo)
    {
        System.Console.Write("Введіть ID товару для видалення: ");
        int id = int.Parse(System.Console.ReadLine() ?? "0");

        _productRepo.Delete(id);
        System.Console.WriteLine("Товар видалено!!");
    }
}