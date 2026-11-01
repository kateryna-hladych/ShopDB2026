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

        [Test]
        public void PasswordsDoNotMatch_ShowsErrorAndDisablesCommand()
        {
            _vm.Username = "ivan";
            _vm.NewPassword = "123456";
            _vm.ConfirmPassword = "654321";

            Assert.That(_vm.ConfirmError, Is.Not.Empty);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Reset_ValidData_RaisesPasswordReset()
        {
            string? resetFor = null;
            _vm.PasswordReset += (_, name) => resetFor = name;

            _vm.Username = "ivan";
            _vm.NewPassword = "newpass99";
            _vm.ConfirmPassword = "newpass99";
            _vm.ResetCommand.Execute(null);

            Assert.That(resetFor, Is.EqualTo("ivan"));
            Assert.That(_auth.ResetCalls, Is.EqualTo(1));
        }

        [Test]
        public void Reset_UnknownUser_ShowsMessage()
        {
            _auth.ResetResult = false;

            _vm.Username = "ghost";
            _vm.NewPassword = "newpass99";
            _vm.ConfirmPassword = "newpass99";
            _vm.ResetCommand.Execute(null);

            Assert.That(_vm.Message, Is.EqualTo("Користувача з таким логіном не знайдено."));
        }
    }
}