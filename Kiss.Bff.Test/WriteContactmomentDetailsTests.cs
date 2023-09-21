using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class WriteContactmomentDetailsTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
          InitializeDatabase();
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task Post_ValidModel_ReturnsOk()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new WriteContactmomentenDetails(dbContext);
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
            var result = await controller.Post(validModel, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual(200, result.StatusCode);

            // Check if the model is added to the database
            var addedModel = await dbContext.ContactMomentDetails.FirstOrDefaultAsync();
            Assert.IsNotNull(addedModel);
            Assert.AreEqual(validModel.Id, addedModel.Id);
        }


        [TestMethod]
        public async Task Post_ModelWithExistingId_UpdatesExistingEntity()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var existingEntity = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1",
                EmailadresKcm = "test@example.com"
            };
            dbContext.ContactMomentDetails.Add(existingEntity);
            dbContext.SaveChanges();

            var controller = new WriteContactmomentenDetails(dbContext);
            var model = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 2",
                Vraag = "Question 2",
                EmailadresKcm = "test@example.com"
            };

            // Act
            var result = await controller.Post(model, CancellationToken.None) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Check if the entity was updated in the database
            var updatedEntity = await dbContext.ContactMomentDetails.FindAsync(model.Id);
            Assert.IsNotNull(updatedEntity);
            Assert.AreEqual(model.Gespreksresultaat, updatedEntity.Gespreksresultaat);
        }
    }
}



