using ShopBL;
using shopViewModels;

namespace shopBL_Tests
{
    [TestFixture]
    public class PasswordRecoveryViewModelTests
    {
        private FakeAuthService _auth = null!;
        private PasswordRecoveryViewModel _vm = null!;

        [SetUp]
        public void SetUp()
        {
            _auth = new FakeAuthService();
            _vm = new PasswordRecoveryViewModel(_auth);
        }

        private void FillValidForm(string username = "ivan", string password = "newpass99")
        {
            _vm.Username = username;
            _vm.NewPassword = password;
            _vm.ConfirmPassword = password;
        }

        [Test]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordRecoveryViewModel(null!));
        }

        

        [Test]
        public void InitialState_NoErrorsShownButFormIsInvalid()
        {
            Assert.That(_vm.UsernameError, Is.Empty);
            Assert.That(_vm.PasswordError, Is.Empty);
            Assert.That(_vm.ConfirmError, Is.Empty);
            Assert.That(_vm.IsValid, Is.False);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
        }

        

        [Test]
        public void Username_OnlySpaces_ShowsError()
        {
            _vm.Username = "   ";

            Assert.That(_vm.UsernameError, Is.Not.Empty);
        }

        [Test]
        public void Username_Filled_NoError()
        {
            _vm.Username = "ivan";

            Assert.That(_vm.UsernameError, Is.Empty);
        }

        [TestCase("1")]
        [TestCase("12345")]
        public void NewPassword_TooShort_ShowsError(string password)
        {
            _vm.NewPassword = password;

            Assert.That(_vm.PasswordError, Does.Contain("мінімум 6"));
        }

        [Test]
        public void NewPassword_ValidLength_NoError()
        {
            _vm.NewPassword = "123456";

            Assert.That(_vm.PasswordError, Is.Empty);
        }

        [Test]
        public void ConfirmPassword_DoesNotMatch_ShowsError()
        {
            _vm.NewPassword = "123456";
            _vm.ConfirmPassword = "654321";

            Assert.That(_vm.ConfirmError, Is.EqualTo("Паролі не збігаються."));
        }

        [Test]
        public void ConfirmPassword_Matches_NoError()
        {
            _vm.NewPassword = "123456";
            _vm.ConfirmPassword = "123456";

            Assert.That(_vm.ConfirmError, Is.Empty);
        }

        [Test]
        public void ChangingNewPassword_RaisesPropertyChangedForConfirmError()
        {
            var changed = new List<string?>();
            _vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            _vm.NewPassword = "123456";

            Assert.That(changed, Does.Contain(nameof(PasswordRecoveryViewModel.ConfirmError)));
        }

        

        [Test]
        public void Form_AllFieldsValid_CommandEnabled()
        {
            FillValidForm();

            Assert.That(_vm.IsValid, Is.True);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Form_PasswordsDoNotMatch_CommandDisabled()
        {
            _vm.Username = "ivan";
            _vm.NewPassword = "newpass99";
            _vm.ConfirmPassword = "different";

            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Form_ShortPassword_CommandDisabled()
        {
            FillValidForm(password: "123");

            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Form_EmptyUsername_CommandDisabled()
        {
            _vm.NewPassword = "newpass99";
            _vm.ConfirmPassword = "newpass99";

            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
        }

        

        [Test]
        public void Reset_Success_RaisesPasswordResetWithTrimmedName()
        {
            string? resetFor = null;
            _vm.PasswordReset += (_, name) => resetFor = name;

            FillValidForm(username: "  ivan  ");
            _vm.ResetCommand.Execute(null);

            Assert.That(resetFor, Is.EqualTo("ivan"));
            Assert.That(_auth.ResetCalls, Is.EqualTo(1));
            Assert.That(_auth.LastUsername, Is.EqualTo("ivan"));
            Assert.That(_auth.LastPassword, Is.EqualTo("newpass99"));
            Assert.That(_vm.Message, Is.Empty);
        }

        [Test]
        public void Reset_UnknownUser_ShowsMessageAndDoesNotRaiseEvent()
        {
            _auth.ResetResult = false;
            bool raised = false;
            _vm.PasswordReset += (_, _) => raised = true;

            FillValidForm();
            _vm.ResetCommand.Execute(null);

            Assert.That(raised, Is.False);
            Assert.That(_vm.Message, Is.EqualTo("Користувача з таким логіном не знайдено."));
        }

        [Test]
        public void Reset_BusinessRuleException_ShowsItsMessage()
        {
            _auth.ExceptionToThrow = new BusinessRuleException("Пароль занадто простий.");

            FillValidForm();
            _vm.ResetCommand.Execute(null);

            Assert.That(_vm.Message, Is.EqualTo("Пароль занадто простий."));
        }

        [Test]
        public void Reset_UnexpectedException_ShowsSaveErrorWithDetails()
        {
            _auth.ExceptionToThrow = new InvalidOperationException("db down");

            FillValidForm();
            _vm.ResetCommand.Execute(null);

            Assert.That(_vm.Message, Does.StartWith("Помилка збереження"));
            Assert.That(_vm.Message, Does.Contain("db down"));
        }

        [Test]
        public void ChangingAnyField_ClearsPreviousMessage()
        {
            _auth.ResetResult = false;
            FillValidForm();
            _vm.ResetCommand.Execute(null);
            Assert.That(_vm.Message, Is.Not.Empty);

            _vm.Username = "olena";

            Assert.That(_vm.Message, Is.Empty);
        }
    }
}