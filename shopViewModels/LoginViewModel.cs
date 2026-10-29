using System;
using System.Windows.Input;
using ShopBL;

namespace shopViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoginCommand = new RelayCommand(_ => Login(),
                _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(Password));
        }

        public string Username { get => _username; set => SetProperty(ref _username, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
        public string Role { get; private set; } = string.Empty;

        public ICommand LoginCommand { get; }

        public event EventHandler? LoginSucceeded;

        private void Login()
        {
            ErrorMessage = string.Empty;
            try
            {
                if (_authService.Authenticate(Username.Trim(), Password, out var role))
                {
                    Role = role;
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Невірний логін або пароль!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Помилка підключення до бази: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}