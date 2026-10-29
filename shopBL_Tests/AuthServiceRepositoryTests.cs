using ShopBL;

namespace shopBL_Tests
{
    [TestFixture]
    public class AuthServiceRepositoryTests
    {
        private FakeUserRepository _repo = null!;
        private PasswordHasher _hasher = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new FakeUserRepository();
            _hasher = new PasswordHasher();
            _authService = new AuthService(_repo, _hasher);
        }

        

        [Test]
        public void Register_NewUser_ReturnsTrueAndAddsUser()
        {
            bool result = _authService.Register("ivan", "password1");

            Assert.That(result, Is.True);
            Assert.That(_repo.Users, Has.Count.EqualTo(1));
            Assert.That(_repo.AddCalls, Is.EqualTo(1));
        }

        [Test]
        public void Register_StoresHashedPassword_NotPlainText()
        {
            _authService.Register("ivan", "password1");

            var user = _repo.Users.Single();

            Assert.That(user.PasswordHash, Is.Not.EqualTo("password1"));
            Assert.That(_hasher.VerifyPassword("password1", user.PasswordHash), Is.True);
        }

        [Test]
        public void Register_WithoutRole_AssignsDefaultUserRole()
        {
            _authService.Register("ivan", "password1");

            Assert.That(_repo.Users.Single().Role, Is.EqualTo("User"));
        }

        [Test]
        public void Register_WithCustomRole_SavesThatRole()
        {
            _authService.Register("boss", "password1", "admin");

            Assert.That(_repo.Users.Single().Role, Is.EqualTo("admin"));
        }

        [Test]
        public void Register_UsernameWithSpaces_IsTrimmedBeforeSaving()
        {
            _authService.Register("   ivan   ", "password1");

            Assert.That(_repo.Users.Single().Username, Is.EqualTo("ivan"));
        }

        [Test]
        public void Register_DuplicateUsername_ReturnsFalseAndDoesNotAddSecond()
        {
            _authService.Register("ivan", "password1");

            bool second = _authService.Register("ivan", "otherpass2");

            Assert.That(second, Is.False);
            Assert.That(_repo.Users, Has.Count.EqualTo(1));
        }

        [Test]
        public void Register_DuplicateUsernameWithSpaces_IsDetectedAfterNormalization()
        {
            _authService.Register("ivan", "password1");

            bool second = _authService.Register("  ivan  ", "otherpass2");

            Assert.That(second, Is.False);
            Assert.That(_repo.Users, Has.Count.EqualTo(1));
        }

        [Test]
        public void Register_TwoDifferentUsers_BothAdded()
        {
            bool first = _authService.Register("ivan", "password1");
            bool second = _authService.Register("olena", "password2");

            Assert.That(first, Is.True);
            Assert.That(second, Is.True);
            Assert.That(_repo.Users, Has.Count.EqualTo(2));
        }

        [Test]
        public void Register_SamePasswordForTwoUsers_ProducesDifferentHashes()
        {
            _authService.Register("ivan", "password1");
            _authService.Register("olena", "password1");

            Assert.That(_repo.Users[0].PasswordHash, Is.Not.EqualTo(_repo.Users[1].PasswordHash));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("ab")]
        [TestCase("abcdefghijklmnopqrstu")] // 21 символ
        public void Register_InvalidUsername_ThrowsBusinessRuleException(string username)
        {
            Assert.Throws<BusinessRuleException>(
                () => _authService.Register(username, "password1"));
        }

        [TestCase("")]
        [TestCase("123")]
        [TestCase("12345")]
        public void Register_InvalidPassword_ThrowsBusinessRuleException(string password)
        {
            Assert.Throws<BusinessRuleException>(
                () => _authService.Register("ivan", password));
        }

        [Test]
        public void Register_InvalidData_DoesNotTouchRepository()
        {
            Assert.Throws<BusinessRuleException>(() => _authService.Register("ab", "123"));

            Assert.That(_repo.LookupCalls, Is.EqualTo(0));
            Assert.That(_repo.AddCalls, Is.EqualTo(0));
        }

        

        [Test]
        public void Authenticate_CorrectCredentials_ReturnsTrueAndRole()
        {
            _authService.Register("ivan", "password1");

            bool result = _authService.Authenticate("ivan", "password1", out string role);

            Assert.That(result, Is.True);
            Assert.That(role, Is.EqualTo("User"));
        }

        [Test]
        public void Authenticate_UsernameWithSpaces_IsNormalized()
        {
            _authService.Register("ivan", "password1");

            bool result = _authService.Authenticate("  ivan ", "password1", out _);

            Assert.That(result, Is.True);
        }

        [Test]
        public void Authenticate_AdminUser_ReturnsAdminRole()
        {
            _authService.Register("boss", "admin123", "admin");

            bool result = _authService.Authenticate("boss", "admin123", out string role);

            Assert.That(result, Is.True);
            Assert.That(role, Is.EqualTo("admin"));
        }

        [Test]
        public void Authenticate_WrongPassword_ReturnsFalseAndEmptyRole()
        {
            _authService.Register("ivan", "password1");

            bool result = _authService.Authenticate("ivan", "wrongpass", out string role);

            Assert.That(result, Is.False);
            Assert.That(role, Is.Empty);
        }

        [Test]
        public void Authenticate_UnknownUser_ReturnsFalse()
        {
            bool result = _authService.Authenticate("ghost", "password1", out string role);

            Assert.That(result, Is.False);
            Assert.That(role, Is.Empty);
        }

        [TestCase("", "password1")]
        [TestCase("ivan", "")]
        [TestCase("   ", "   ")]
        public void Authenticate_EmptyFields_ReturnsFalseWithoutLookup(string username, string password)
        {
            bool result = _authService.Authenticate(username, password, out _);

            Assert.That(result, Is.False);
            Assert.That(_repo.LookupCalls, Is.EqualTo(0));
        }

        [Test]
        public void Authenticate_PasswordIsCaseSensitive()
        {
            _authService.Register("ivan", "Password1");

            bool result = _authService.Authenticate("ivan", "password1", out _);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Authenticate_UserWithStoredPlainPassword_IsRejected()
        {
            _repo.Add(new ShopDB.DAL.EF.Entities.User
            {
                Username = "legacy",
                PasswordHash = "plainpassword",
                Role = "User"
            });

            bool result = _authService.Authenticate("legacy", "plainpassword", out _);

            Assert.That(result, Is.False);
        }

        

        [Test]
        public void ResetPassword_ExistingUser_NewPasswordWorksOldDoesNot()
        {
            _authService.Register("ivan", "password1");

            bool reset = _authService.ResetPassword("ivan", "newpass99");

            Assert.That(reset, Is.True);
            Assert.That(_authService.Authenticate("ivan", "newpass99", out _), Is.True);
            Assert.That(_authService.Authenticate("ivan", "password1", out _), Is.False);
        }

        [Test]
        public void ResetPassword_StoresHashAndCallsUpdateOnce()
        {
            _authService.Register("ivan", "password1");

            _authService.ResetPassword("ivan", "newpass99");

            var user = _repo.Users.Single();
            Assert.That(user.PasswordHash, Is.Not.EqualTo("newpass99"));
            Assert.That(_hasher.VerifyPassword("newpass99", user.PasswordHash), Is.True);
            Assert.That(_repo.UpdateCalls, Is.EqualTo(1));
        }

        [Test]
        public void ResetPassword_UnknownUser_ReturnsFalseAndDoesNotUpdate()
        {
            bool result = _authService.ResetPassword("ghost", "newpass99");

            Assert.That(result, Is.False);
            Assert.That(_repo.UpdateCalls, Is.EqualTo(0));
        }

        [Test]
        public void ResetPassword_ShortPassword_ThrowsBusinessRuleException()
        {
            _authService.Register("ivan", "password1");

            Assert.Throws<BusinessRuleException>(
                () => _authService.ResetPassword("ivan", "123"));
        }

        [Test]
        public void ResetPassword_DoesNotAffectOtherUsers()
        {
            _authService.Register("ivan", "password1");
            _authService.Register("olena", "password2");

            _authService.ResetPassword("ivan", "newpass99");

            Assert.That(_authService.Authenticate("olena", "password2", out _), Is.True);
        }

        

        [Test]
        public void ChangePassword_CorrectOldPassword_ReturnsTrueAndNewPasswordWorks()
        {
            _authService.Register("ivan", "password1");

            bool result = _authService.ChangePassword("ivan", "password1", "newpass99");

            Assert.That(result, Is.True);
            Assert.That(_authService.Authenticate("ivan", "newpass99", out _), Is.True);
            Assert.That(_authService.Authenticate("ivan", "password1", out _), Is.False);
        }

        [Test]
        public void ChangePassword_WrongOldPassword_ReturnsFalseAndKeepsOldPassword()
        {
            _authService.Register("ivan", "password1");

            bool result = _authService.ChangePassword("ivan", "wrongold", "newpass99");

            Assert.That(result, Is.False);
            Assert.That(_repo.UpdateCalls, Is.EqualTo(0));
            Assert.That(_authService.Authenticate("ivan", "password1", out _), Is.True);
        }

        [Test]
        public void ChangePassword_UnknownUser_ReturnsFalse()
        {
            bool result = _authService.ChangePassword("ghost", "password1", "newpass99");

            Assert.That(result, Is.False);
        }

        [Test]
        public void ChangePassword_ShortNewPassword_ThrowsBusinessRuleException()
        {
            _authService.Register("ivan", "password1");

            Assert.Throws<BusinessRuleException>(
                () => _authService.ChangePassword("ivan", "password1", "123"));
        }
    }
}