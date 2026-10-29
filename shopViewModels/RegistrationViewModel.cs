using System;
using System.Windows.Input;
using ShopBL;

namespace shopViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;

        public RegistrationViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            RegisterCommand = new RelayCommand(_ => Register(), _ => IsValid);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    Message = string.Empty;
                    OnPropertyChanged(nameof(UsernameError));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    Message = string.Empty;
                    OnPropertyChanged(nameof(PasswordError));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Message { get => _message; private set => SetProperty(ref _message, value); }

        
        public string UsernameError => Username.Length == 0 ? string.Empty : ValidateUsername();
        public string PasswordError => Password.Length == 0 ? string.Empty : ValidatePassword();

        public bool IsValid => ValidateUsername().Length == 0 && ValidatePassword().Length == 0;

        public ICommand RegisterCommand { get; }

        public event EventHandler<string>? Registered;

        private string ValidateUsername()
        {
            var u = Username.Trim();
            if (u.Length == 0) return "Логін обов'язковий.";
            if (u.Length < 3 || u.Length > 20) return "Логін: від 3 до 20 символів.";
            return string.Empty;
        }

        private string ValidatePassword()
        {
            if (Password.Length == 0) return "Пароль обов'язковий.";
            if (Password.Length < 6) return "Пароль: мінімум 6 символів.";
            return string.Empty;
        }

        
        private void Register()
        {
            try
            {
                var name = Username.Trim();
                if (_authService.Register(name, Password))
                    Registered?.Invoke(this, name);
                else
                    Message = "Такий користувач уже існує.";
            }
            catch (BusinessRuleException ex)
            {
                Message = ex.Message;
            }
            catch (Exception ex)
            {
                Message = "Помилка збереження: " + (ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}