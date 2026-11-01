using ShopBL;

namespace shopBL_Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private FakeUserRepository _repo = null!;
        private PasswordHasher _hasher = null!;
        private AuthService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new FakeUserRepository();
            _hasher = new PasswordHasher();
            _service = new AuthService(_repo, _hasher);
        }

        [Test]
        public void Register_NewUser_SavesUserWithDefaultUserRole()
        {
            bool result = _service.Register("ivan", "password1");

            Assert.That(result, Is.True);
            Assert.That(_repo.Users.Single().Username, Is.EqualTo("ivan"));
            Assert.That(_repo.Users.Single().Role, Is.EqualTo("User"));
        }

        [Test]
        public void Register_StoresHashedPassword_NotPlainText()
        {
            _service.Register("ivan", "password1");

            var user = _repo.Users.Single();

            Assert.That(user.PasswordHash, Is.Not.EqualTo("password1"));
            Assert.That(_hasher.VerifyPassword("password1", user.PasswordHash), Is.True);
        }

        [Test]
        public void Register_DuplicateUsername_ReturnsFalseAndDoesNotAddSecondUser()
        {
            _service.Register("ivan", "password1");

            bool second = _service.Register("ivan", "otherpass2");

            Assert.That(second, Is.False);
            Assert.That(_repo.Users, Has.Count.EqualTo(1));
        }

        [Test]
        public void Register_UsernameTooShort_ThrowsBusinessRuleException()
        {
            Assert.Throws<BusinessRuleException>(() => _service.Register("ab", "password1"));
        }

        [Test]
        public void Register_PasswordTooShort_ThrowsBusinessRuleException()
        {
            Assert.Throws<BusinessRuleException>(() => _service.Register("ivan", "123"));
        }

        [Test]
        public void Authenticate_ProductManager_ReturnsTrueAndManagerRole()
        {
            _service.Register("manager", "manager123", "ProductManager");

            bool result = _service.Authenticate("manager", "manager123", out string role);

            Assert.That(result, Is.True);
            Assert.That(role, Is.EqualTo("ProductManager"));
        }

        [Test]
        public void Authenticate_WrongPassword_ReturnsFalse()
        {
            _service.Register("ivan", "password1");

            bool result = _service.Authenticate("ivan", "wrongpass", out string role);

            Assert.That(result, Is.False);
            Assert.That(role, Is.Empty);
        }

        [Test]
        public void Authenticate_UnknownUser_ReturnsFalse()
        {
            bool result = _service.Authenticate("ghost", "password1", out _);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ResetPassword_ExistingUser_NewPasswordWorksOldDoesNot()
        {
            _service.Register("ivan", "password1");

            bool reset = _service.ResetPassword("ivan", "newpass99");

            Assert.That(reset, Is.True);
            Assert.That(_service.Authenticate("ivan", "newpass99", out _), Is.True);
            Assert.That(_service.Authenticate("ivan", "password1", out _), Is.False);
        }

        [Test]
        public void ChangePassword_WrongOldPassword_ReturnsFalseAndKeepsOldPassword()
        {
            _service.Register("ivan", "password1");

            bool result = _service.ChangePassword("ivan", "wrongold", "newpass99");

            Assert.That(result, Is.False);
            Assert.That(_service.Authenticate("ivan", "password1", out _), Is.True);
        }
    }
}