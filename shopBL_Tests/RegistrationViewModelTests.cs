using shopViewModels;

namespace shopBL_Tests
{
    [TestFixture]
    public class RegistrationViewModelTests
    {
        private FakeAuthService _auth = null!;
        private RegistrationViewModel _vm = null!;

        [SetUp]
        public void SetUp()
        {
            _auth = new FakeAuthService();
            _vm = new RegistrationViewModel(_auth);
        }

        [Test]
        public void Username_TooShort_ShowsValidationError()
        {
            _vm.Username = "ab";

            Assert.That(_vm.UsernameError, Is.Not.Empty);
        }

        [Test]
        public void Password_TooShort_ShowsValidationError()
        {
            _vm.Password = "123";

            Assert.That(_vm.PasswordError, Is.Not.Empty);
        }

        [Test]
        public void ValidData_EnablesRegisterCommand()
        {
            Assert.That(_vm.RegisterCommand.CanExecute(null), Is.False);

            _vm.Username = "ivan";
            _vm.Password = "123456";

            Assert.That(_vm.RegisterCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Register_Success_RaisesRegisteredEventWithTrimmedName()
        {
            string? registered = null;
            _vm.Registered += (_, name) => registered = name;

            _vm.Username = "  ivan  ";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(registered, Is.EqualTo("ivan"));
            Assert.That(_auth.RegisterCalls, Is.EqualTo(1));
        }

        [Test]
        public void Register_DuplicateUser_ShowsMessage()
        {
            _auth.RegisterResult = false;

            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(_vm.Message, Is.EqualTo("Такий користувач уже існує."));
        }
    }
}