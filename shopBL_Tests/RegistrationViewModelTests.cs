using ShopBL;
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
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrationViewModel(null!));
        }

        

        [Test]
        public void InitialState_NoErrorsShownButFormIsInvalid()
        {
            Assert.That(_vm.UsernameError, Is.Empty);
            Assert.That(_vm.PasswordError, Is.Empty);
            Assert.That(_vm.IsValid, Is.False);
            Assert.That(_vm.RegisterCommand.CanExecute(null), Is.False);
        }

        

        [TestCase("a")]
        [TestCase("ab")]
        [TestCase("abcdefghijklmnopqrstu")] 
        public void Username_WrongLength_ShowsError(string username)
        {
            _vm.Username = username;

            Assert.That(_vm.UsernameError, Does.Contain("від 3 до 20"));
        }

        [TestCase("abc")]
        [TestCase("ivan")]
        [TestCase("abcdefghijklmnopqrst")] 
        public void Username_ValidLength_NoError(string username)
        {
            _vm.Username = username;

            Assert.That(_vm.UsernameError, Is.Empty);
        }

        [Test]
        public void Username_OnlySpaces_ShowsError()
        {
            _vm.Username = "     ";

            Assert.That(_vm.UsernameError, Is.Not.Empty);
        }

        [Test]
        public void Username_SpacesAroundValidName_AreIgnoredByValidation()
        {
            _vm.Username = "  ivan  ";

            Assert.That(_vm.UsernameError, Is.Empty);
        }

        

        [TestCase("1")]
        [TestCase("12345")]
        public void Password_TooShort_ShowsError(string password)
        {
            _vm.Password = password;

            Assert.That(_vm.PasswordError, Does.Contain("мінімум 6"));
        }

        [TestCase("123456")]
        [TestCase("a-very-long-password")]
        public void Password_ValidLength_NoError(string password)
        {
            _vm.Password = password;

            Assert.That(_vm.PasswordError, Is.Empty);
        }

        

        [Test]
        public void Form_ValidUsernameAndPassword_IsValidAndCommandEnabled()
        {
            _vm.Username = "ivan";
            _vm.Password = "123456";

            Assert.That(_vm.IsValid, Is.True);
            Assert.That(_vm.RegisterCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Form_ValidUsernameButShortPassword_CommandDisabled()
        {
            _vm.Username = "ivan";
            _vm.Password = "123";

            Assert.That(_vm.IsValid, Is.False);
            Assert.That(_vm.RegisterCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Form_ShortUsernameButValidPassword_CommandDisabled()
        {
            _vm.Username = "iv";
            _vm.Password = "123456";

            Assert.That(_vm.RegisterCommand.CanExecute(null), Is.False);
        }

        

        [Test]
        public void Register_Success_RaisesRegisteredWithTrimmedName()
        {
            string? registeredName = null;
            _vm.Registered += (_, name) => registeredName = name;

            _vm.Username = "  ivan  ";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(registeredName, Is.EqualTo("ivan"));
            Assert.That(_auth.LastUsername, Is.EqualTo("ivan"));
            Assert.That(_auth.LastPassword, Is.EqualTo("123456"));
            Assert.That(_vm.Message, Is.Empty);
        }

        [Test]
        public void Register_DuplicateUser_ShowsMessageAndDoesNotRaiseEvent()
        {
            _auth.RegisterResult = false;
            bool raised = false;
            _vm.Registered += (_, _) => raised = true;

            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(raised, Is.False);
            Assert.That(_vm.Message, Is.EqualTo("Такий користувач уже існує."));
        }

        [Test]
        public void Register_BusinessRuleException_ShowsItsMessage()
        {
            _auth.ExceptionToThrow = new BusinessRuleException("Пароль занадто простий.");

            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(_vm.Message, Is.EqualTo("Пароль занадто простий."));
        }

        [Test]
        public void Register_UnexpectedException_ShowsSaveErrorWithDetails()
        {
            _auth.ExceptionToThrow = new InvalidOperationException("db down");

            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(_vm.Message, Does.StartWith("Помилка збереження"));
            Assert.That(_vm.Message, Does.Contain("db down"));
        }

        [Test]
        public void Register_UnexpectedExceptionWithInner_ShowsInnerMessage()
        {
            _auth.ExceptionToThrow = new Exception("outer", new Exception("inner problem"));

            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            Assert.That(_vm.Message, Does.Contain("inner problem"));
        }

        

        [Test]
        public void ChangingUsername_ClearsPreviousMessage()
        {
            _auth.RegisterResult = false;
            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);
            Assert.That(_vm.Message, Is.Not.Empty);

            _vm.Username = "ivan2";

            Assert.That(_vm.Message, Is.Empty);
        }

        [Test]
        public void ChangingPassword_ClearsPreviousMessage()
        {
            _auth.RegisterResult = false;
            _vm.Username = "ivan";
            _vm.Password = "123456";
            _vm.RegisterCommand.Execute(null);

            _vm.Password = "654321";

            Assert.That(_vm.Message, Is.Empty);
        }

        [Test]
        public void ChangingUsername_RaisesPropertyChangedForUsernameError()
        {
            var changed = new List<string?>();
            _vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            _vm.Username = "ivan";

            Assert.That(changed, Does.Contain(nameof(RegistrationViewModel.UsernameError)));
        }

        [Test]
        public void ChangingPassword_RaisesPropertyChangedForPasswordError()
        {
            var changed = new List<string?>();
            _vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            _vm.Password = "123456";

            Assert.That(changed, Does.Contain(nameof(RegistrationViewModel.PasswordError)));
        }
    }
}