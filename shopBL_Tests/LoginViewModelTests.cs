using shopViewModels;

namespace shopBL_Tests
{
    [TestFixture]
    public class LoginViewModelTests
    {
        private FakeAuthService _auth = null!;
        private LoginViewModel _vm = null!;

        [SetUp]
        public void SetUp()
        {
            _auth = new FakeAuthService();
            _vm = new LoginViewModel(_auth);
        }

        [Test]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginViewModel(null!));
        }

        

        [Test]
        public void LoginCommand_EmptyFields_CannotExecute()
        {
            Assert.That(_vm.LoginCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void LoginCommand_OnlyUsername_CannotExecute()
        {
            _vm.Username = "admin";

            Assert.That(_vm.LoginCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void LoginCommand_OnlyPassword_CannotExecute()
        {
            _vm.Password = "admin123";

            Assert.That(_vm.LoginCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void LoginCommand_WhitespaceUsername_CannotExecute()
        {
            _vm.Username = "   ";
            _vm.Password = "admin123";

            Assert.That(_vm.LoginCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void LoginCommand_BothFieldsFilled_CanExecute()
        {
            _vm.Username = "admin";
            _vm.Password = "admin123";

            Assert.That(_vm.LoginCommand.CanExecute(null), Is.True);
        }

        

        [Test]
        public void Login_ValidCredentials_RaisesLoginSucceededAndSetsRole()
        {
            _auth.AuthenticateResult = true;
            _auth.RoleToReturn = "admin";
            bool raised = false;
            _vm.LoginSucceeded += (_, _) => raised = true;

            _vm.Username = "admin";
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.That(raised, Is.True);
            Assert.That(_vm.Role, Is.EqualTo("admin"));
            Assert.That(_vm.ErrorMessage, Is.Empty);
        }

        [Test]
        public void Login_UsernameWithSpaces_IsTrimmedBeforeCallingService()
        {
            _auth.AuthenticateResult = true;

            _vm.Username = "  admin  ";
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.That(_auth.LastUsername, Is.EqualTo("admin"));
            Assert.That(_auth.LastPassword, Is.EqualTo("admin123"));
        }

        [Test]
        public void Login_ValidCredentials_CallsServiceExactlyOnce()
        {
            _auth.AuthenticateResult = true;

            _vm.Username = "admin";
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.That(_auth.AuthenticateCalls, Is.EqualTo(1));
        }

        

        [Test]
        public void Login_WrongCredentials_SetsErrorAndDoesNotRaiseEvent()
        {
            _auth.AuthenticateResult = false;
            bool raised = false;
            _vm.LoginSucceeded += (_, _) => raised = true;

            _vm.Username = "admin";
            _vm.Password = "wrong";
            _vm.LoginCommand.Execute(null);

            Assert.That(raised, Is.False);
            Assert.That(_vm.ErrorMessage, Is.EqualTo("Невірний логін або пароль!"));
            Assert.That(_vm.Role, Is.Empty);
        }

        [Test]
        public void Login_SuccessAfterFailure_ClearsErrorMessage()
        {
            _vm.Username = "admin";
            _vm.Password = "wrong";
            _auth.AuthenticateResult = false;
            _vm.LoginCommand.Execute(null);
            Assert.That(_vm.ErrorMessage, Is.Not.Empty);

            _auth.AuthenticateResult = true;
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.That(_vm.ErrorMessage, Is.Empty);
        }

        [Test]
        public void Login_ServiceThrows_ShowsErrorAndDoesNotRaiseEvent()
        {
            _auth.ExceptionToThrow = new InvalidOperationException("db down");
            bool raised = false;
            _vm.LoginSucceeded += (_, _) => raised = true;

            _vm.Username = "admin";
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.That(raised, Is.False);
            Assert.That(_vm.ErrorMessage, Does.Contain("db down"));
        }

        [Test]
        public void Login_ServiceThrowsWithInnerException_ShowsInnerMessage()
        {
            _auth.ExceptionToThrow = new Exception("outer", new Exception("inner problem"));

            _vm.Username = "admin";
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.That(_vm.ErrorMessage, Does.Contain("inner problem"));
        }

        

        [Test]
        public void Username_WhenChanged_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (_, e) => changed = e.PropertyName;

            _vm.Username = "admin";

            Assert.That(changed, Is.EqualTo(nameof(LoginViewModel.Username)));
        }

        [Test]
        public void Password_WhenChanged_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (_, e) => changed = e.PropertyName;

            _vm.Password = "secret";

            Assert.That(changed, Is.EqualTo(nameof(LoginViewModel.Password)));
        }
    }
}