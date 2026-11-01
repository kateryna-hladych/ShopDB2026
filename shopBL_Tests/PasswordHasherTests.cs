using ShopBL;

namespace shopBL_Tests
{
    [TestFixture]
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Test]
        public void HashPassword_DoesNotStorePlainText()
        {
            string hash = _hasher.HashPassword("secret123");

            Assert.That(hash, Is.Not.EqualTo("secret123"));
        }

        [Test]
        public void HashPassword_SamePasswordTwice_GivesDifferentHashesBecauseOfSalt()
        {
            string first = _hasher.HashPassword("secret123");
            string second = _hasher.HashPassword("secret123");

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            string hash = _hasher.HashPassword("secret123");

            Assert.That(_hasher.VerifyPassword("secret123", hash), Is.True);
        }

        [Test]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            string hash = _hasher.HashPassword("secret123");

            Assert.That(_hasher.VerifyPassword("other456", hash), Is.False);
        }
    }
}