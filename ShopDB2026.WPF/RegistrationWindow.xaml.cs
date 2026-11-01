using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using shopViewModels;

namespace ShopDB2026.WPF;

public partial class RegistrationWindow : Window
{
    private readonly RegistrationViewModel _viewModel;

    public RegistrationWindow()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider!.GetRequiredService<RegistrationViewModel>();
        _viewModel.Registered += (_, name) =>
        {
            MessageBox.Show($"Користувача '{name}' зареєстровано!", "Успіх");
            Close();
        };
        DataContext = _viewModel;
    }

    private void PwdRegPassword_PasswordChanged(object sender, RoutedEventArgs e)
        => _viewModel.Password = pwdRegPassword.Password;
}