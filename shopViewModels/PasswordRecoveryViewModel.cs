using System;
using System.Windows.Input;
using ShopBL;

namespace shopViewModels
{
    public class PasswordRecoveryViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _message = string.Empty;

        public PasswordRecoveryViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            ResetCommand = new RelayCommand(_ => Reset(), _ => IsValid);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    Refresh(nameof(UsernameError));
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                {
                    Refresh(nameof(PasswordError), nameof(ConfirmError));
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    Refresh(nameof(ConfirmError));
                }
            }
        }

        public string Message { get => _message; private set => SetProperty(ref _message, value); }

        
        public string UsernameError => Username.Length == 0 ? string.Empty : ValidateUsername();
        public string PasswordError => NewPassword.Length == 0 ? string.Empty : ValidatePassword();
        public string ConfirmError => ConfirmPassword.Length == 0 ? string.Empty : ValidateConfirm();

        public bool IsValid =>
            ValidateUsername().Length == 0 &&
            ValidatePassword().Length == 0 &&
            ValidateConfirm().Length == 0;

        public ICommand ResetCommand { get; }

        public event EventHandler<string>? PasswordReset;

        private void Refresh(params string[] properties)
        {
            Message = string.Empty;
            foreach (var p in properties) OnPropertyChanged(p);
            CommandManager.InvalidateRequerySuggested();
        }

        private string ValidateUsername()
            => string.IsNullOrWhiteSpace(Username) ? "Введіть логін." : string.Empty;

        private string ValidatePassword()
        {
            if (NewPassword.Length == 0) return "Введіть новий пароль.";
            if (NewPassword.Length < 6) return "Пароль: мінімум 6 символів.";
            return string.Empty;
        }

        private string ValidateConfirm()
            => ConfirmPassword != NewPassword ? "Паролі не збігаються." : string.Empty;

       
        private void Reset()
        {
            try
            {
                var name = Username.Trim();
                if (_authService.ResetPassword(name, NewPassword))
                    PasswordReset?.Invoke(this, name);
                else
                    Message = "Користувача з таким логіном не знайдено.";
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