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
        public void LoginCommand_IsDisabledUntilBothFieldsAreFilled()
        {
            Assert.That(_vm.LoginCommand.CanExecute(null), Is.False);

            _vm.Username = "manager";
            _vm.Password = "manager123";

            Assert.That(_vm.LoginCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Login_ValidCredentials_RaisesLoginSucceededWithRole()
        {
            _auth.AuthenticateResult = true;
            _auth.RoleToReturn = "ProductManager";
            bool raised = false;
            _vm.LoginSucceeded += (_, _) => raised = true;

            _vm.Username = "manager";
            _vm.Password = "manager123";
            _vm.LoginCommand.Execute(null);

            Assert.That(raised, Is.True);
            Assert.That(_vm.Role, Is.EqualTo("ProductManager"));
        }

        [Test]
        public void Login_WrongCredentials_ShowsErrorAndDoesNotRaiseEvent()
        {
            _auth.AuthenticateResult = false;
            bool raised = false;
            _vm.LoginSucceeded += (_, _) => raised = true;

            _vm.Username = "manager";
            _vm.Password = "wrong";
            _vm.LoginCommand.Execute(null);

            Assert.That(raised, Is.False);
            Assert.That(_vm.ErrorMessage, Is.EqualTo("Невірний логін або пароль!"));
        }

        [Test]
        public void Login_ServiceThrows_ShowsErrorMessage()
        {
            _auth.ExceptionToThrow = new InvalidOperationException("db down");

            _vm.Username = "manager";
            _vm.Password = "manager123";
            _vm.LoginCommand.Execute(null);

            Assert.That(_vm.ErrorMessage, Does.Contain("db down"));
        }
    }
}