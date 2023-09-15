using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class WriteContactmomentDetailsTests
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
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task Post_ValidModel_ReturnsOk()
        {
            // Arrange
            var controller = new WriteContactmomentenDetails(_dbContext);
            var validModel = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1",
                EmailadresKcm = "test@example.com" // User.GetEmail()
            };

            // Act
            var result = await controller.Post(validModel, CancellationToken.None) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Check if the model is added to the database
            var addedModel = await _dbContext.ContactMomentDetails.FirstOrDefaultAsync();
            Assert.IsNotNull(addedModel);
            Assert.AreEqual(validModel.Id, addedModel.Id);
        }

        [TestMethod]
        public async Task Post_ModelWithDuplicateId_ReturnsOk()
        {
            // Arrange
            var controller = new WriteContactmomentenDetails(_dbContext);
            var model1 = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1",
                EmailadresKcm = "test@example.com" // User.GetEmail()
            };

            var model2 = new ContactmomentDetails
            {
                Id = "1", // Duplicate ID
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 2",
                Vraag = "Question 2",
                EmailadresKcm = "test@example.com" // User.GetEmail()
            };

            // Act
            await controller.Post(model1, CancellationToken.None);
            var result = await controller.Post(model2, CancellationToken.None) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Check if the model with the duplicate ID is updated
            var updatedModel = await _dbContext.ContactMomentDetails.FirstOrDefaultAsync();
            Assert.IsNotNull(updatedModel);
            Assert.AreEqual(model2.Gespreksresultaat, updatedModel.Gespreksresultaat);
        }
    }
}

