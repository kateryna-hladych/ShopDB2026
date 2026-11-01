using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ShopBL;
using ShopDB.DAL.EF.Data;
using shopViewModels;
using ShopDB.DAL.EF.Repositories;
using ShopDB.DAL.EF.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
namespace ShopDB2026.WPF;

public partial class App : Application
{
    public static ServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        
        //services.AddDbContext<ShopDbContext>(options =>
        //options.UseSqlServer("Server=DESKTOP-O20AE25;Database=ShopDB;Trusted_Connection=True;TrustServerCertificate=True;"));

        // Рядок підключення береться з appsettings.json, а не з коду
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<ShopDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IProductService, ProductService>();
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IProductHistoryService, ProductHistoryService>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<PasswordRecoveryViewModel>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<IProductRepository, ProductRepository>();
        services.AddTransient<IProductHistoryRepository, ProductHistoryRepository>();
        ServiceProvider = services.BuildServiceProvider();

        
        try
        {
            var auth = ServiceProvider.GetRequiredService<IAuthService>();

            
            auth.Register("admin", "admin123", "admin");
            auth.Register("manager", "manager123", "ProductManager");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка бази даних:\n{ex.InnerException?.Message ?? ex.Message}", "Помилка");
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        ShowLogin();
    }

    private void ShowLogin()
    {
        var loginWindow = new LoginWindow();

        if (loginWindow.ShowDialog() == true)
        {
            var mainWindow = new MainWindow(loginWindow.Role);
            MainWindow = mainWindow;

            mainWindow.Closed += (_, _) =>
            {
                if (mainWindow.LoggedOut)
                    ShowLogin();   
                else
                    Shutdown();    
            };

            mainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }
}