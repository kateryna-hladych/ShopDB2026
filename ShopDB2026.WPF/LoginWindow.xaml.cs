using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using shopViewModels;

namespace ShopDB2026.WPF
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public string Role { get; private set; } = "user";

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = App.ServiceProvider!.GetRequiredService<LoginViewModel>();
            DataContext = _viewModel;

            _viewModel.LoginSucceeded += (_, _) =>
            {
                Role = _viewModel.Role;
                DialogResult = true;
            };
        }

        
        private void PwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
            => _viewModel.Password = pwdPassword.Password;

        private void TxtForgotPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var recoveryWindow = new PasswordRecoveryWindow();
            recoveryWindow.ShowDialog();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegistrationWindow();
            registerWindow.ShowDialog();
        }
    }
}