using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using shopViewModels;

namespace ShopDB2026.WPF
{
    public partial class PasswordRecoveryWindow : Window
    {
        private readonly PasswordRecoveryViewModel _viewModel;

        public PasswordRecoveryWindow()
        {
            InitializeComponent();

            _viewModel = App.ServiceProvider!.GetRequiredService<PasswordRecoveryViewModel>();
            _viewModel.PasswordReset += (_, name) =>
            {
                MessageBox.Show($"Пароль для '{name}' успішно змінено!", "Успіх",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            };
            DataContext = _viewModel;
        }

        private void PwdNew_PasswordChanged(object sender, RoutedEventArgs e)
            => _viewModel.NewPassword = pwdNew.Password;

        private void PwdConfirm_PasswordChanged(object sender, RoutedEventArgs e)
            => _viewModel.ConfirmPassword = pwdConfirm.Password;
    }
}