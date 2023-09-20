using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ReadContactmomentenDetailsTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
            SeedTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        private void SeedTestData()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var contactmoment1 = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1"
            };

            var contactmoment2 = new ContactmomentDetails
            {
                Id = "2",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 2",
                Vraag = "Question 2"
            };

            dbContext.ContactMomentDetails.AddRange(contactmoment1, contactmoment2);
            dbContext.SaveChanges();
        }

        [TestMethod]
        public async Task Get_ValidId_ReturnsOk()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new ReadContactmomentenDetails(dbContext);
            var validId = "1";

            // Act
            var result = await controller.Get(validId, CancellationToken.None) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var contactmoment = result.Value as ContactmomentDetails;
            Assert.IsNotNull(contactmoment);
            Assert.AreEqual(validId, contactmoment.Id);
        }

        [TestMethod]
        public async Task Get_InvalidId_ReturnsNotFound()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new ReadContactmomentenDetails(dbContext);
            var invalidId = "nonexistent";

            // Act
            var result = await controller.Get(invalidId, CancellationToken.None) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [TestMethod]
        public void Get_All_ReturnsOkWithContactmomenten()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new ReadContactmomentenDetails(dbContext);

            // Act
            var result = controller.Get() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var contactmomenten = result.Value as IEnumerable<ContactmomentDetails>;
            Assert.IsNotNull(contactmomenten);

            var contactmomentList = contactmomenten.ToList();
            Assert.IsNotNull(contactmomentList);
            Assert.AreEqual(2, contactmomentList.Count());
        }
    }
}
