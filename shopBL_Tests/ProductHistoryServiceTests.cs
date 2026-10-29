using ShopBL;
using ShopDB.DAL.EF.Entities;

namespace shopBL_Tests
{
    [TestFixture]
    public class ProductHistoryServiceTests
    {
        private FakeProductHistoryRepository _histories = null!;
        private ProductHistoryService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _histories = new FakeProductHistoryRepository();
            _service = new ProductHistoryService(_histories);
        }

        [Test]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductHistoryService(null!));
        }

        [Test]
        public void GetAllHistories_EmptyRepository_ReturnsEmptyCollection()
        {
            Assert.That(_service.GetAllHistories(), Is.Empty);
        }

        [Test]
        public void AddHistory_ValidRecord_IsPassedToRepository()
        {
            _service.AddHistory(new ProductHistory
            {
                ProductId = 1,
                AddedQuantity = 5,
                Comment = "Надходження від постачальника",
                CreatedAt = DateTime.Now
            });

            Assert.That(_histories.AddCalls, Is.EqualTo(1));
        }

        [Test]
        public void AddHistory_ValidRecord_SavesAllFields()
        {
            var date = new DateTime(2026, 10, 28, 12, 0, 0);

            _service.AddHistory(new ProductHistory
            {
                ProductId = 7,
                AddedQuantity = 12,
                Comment = "Нова партія смартфонів",
                CreatedAt = date
            });

            var saved = _histories.Items.Single();

            Assert.That(saved.ProductId, Is.EqualTo(7));
            Assert.That(saved.AddedQuantity, Is.EqualTo(12));
            Assert.That(saved.Comment, Is.EqualTo("Нова партія смартфонів"));
            Assert.That(saved.CreatedAt, Is.EqualTo(date));
        }

        [Test]
        public void AddHistory_ValidRecord_GetsGeneratedId()
        {
            var history = new ProductHistory
            {
                ProductId = 1,
                AddedQuantity = 5,
                Comment = "Надходження від постачальника",
                CreatedAt = DateTime.Now
            };

            _service.AddHistory(history);

            Assert.That(history.Id, Is.GreaterThan(0));
        }

        [Test]
        public void AddHistory_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.AddHistory(null!));
        }

        [Test]
        public void GetAllHistories_AfterAddingSeveral_ReturnsAllOfThem()
        {
            for (int i = 1; i <= 3; i++)
            {
                _service.AddHistory(new ProductHistory
                {
                    ProductId = i,
                    AddedQuantity = i * 10,
                    Comment = $"Надходження #{i}",
                    CreatedAt = DateTime.Now
                });
            }

            var result = _service.GetAllHistories().ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.Select(h => h.ProductId), Is.EquivalentTo(new[] { 1, 2, 3 }));
        }
    }
}