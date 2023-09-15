using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ReadContactmomentenDetailsTests
    {
        private BeheerDbContext _dbContext;

        [TestInitialize]
        public void Initialize()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _dbContext = new BeheerDbContext(options);
            SeedTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void SeedTestData()
        {
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

            _dbContext.ContactMomentDetails.AddRange(contactmoment1, contactmoment2);
            _dbContext.SaveChanges();
        }

        [TestMethod]
        public async Task Get_ValidId_ReturnsOk()
        {
            // Arrange
            var controller = new ReadContactmomentenDetails(_dbContext);
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
            // Arrange
            var controller = new ReadContactmomentenDetails(_dbContext);
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
            // Arrange
            var controller = new ReadContactmomentenDetails(_dbContext);

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
