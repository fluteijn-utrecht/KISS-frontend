using Kiss.Bff.Beheer.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Features;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetFeaturedCountControllerTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        [TestMethod]
        public async Task GetCount_ReturnsActionResult_WithFeaturedCount()
        {
            // Arrange
            var berichten = new List<Bericht>
        {
            new Bericht { IsBelangrijk = true },
            new Bericht { IsBelangrijk = true },
                new Bericht { IsBelangrijk = false }
            };

            using var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Berichten.AddRange(berichten);
            dbContext.SaveChanges();

            var controller = new GetFeaturedCountController(dbContext);

            // Act
            var result = await controller.GetCount(new CancellationToken());

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<FeaturedCount>));
            Assert.IsInstanceOfType(result.Value, typeof(FeaturedCount));
            var featuredCount = result.Value;
            Assert.AreEqual(2, featuredCount.Count);
        }
    }
}
