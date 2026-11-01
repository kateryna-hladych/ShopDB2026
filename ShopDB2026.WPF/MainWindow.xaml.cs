using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ShopBL;
using shopViewModels;

namespace ShopDB2026.WPF;

public partial class MainWindow : Window
{
    
    public bool LoggedOut { get; private set; }

    public MainWindow(string role)
    {
        InitializeComponent();

        var productService = App.ServiceProvider!.GetRequiredService<IProductService>();
        var categoryService = App.ServiceProvider!.GetRequiredService<ICategoryService>();
        var historyService = App.ServiceProvider!.GetRequiredService<IProductHistoryService>();

        var viewModel = new MainViewModel(role, productService, categoryService, historyService);

        viewModel.LogoutRequested += (_, _) =>
        {
            var answer = MessageBox.Show("Вийти з облікового запису?", "Вихід",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                LoggedOut = true;
                Close();
            }
        };

        DataContext = viewModel;
    }
}