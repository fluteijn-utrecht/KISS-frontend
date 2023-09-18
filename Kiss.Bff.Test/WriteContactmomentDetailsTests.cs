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
        private DbContextOptions<BeheerDbContext> _dbContextOptions;

        [TestInitialize]
        public void Initialize()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            _dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _dbContext = new BeheerDbContext(_dbContextOptions);
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
            var result = await controller.Post(validModel, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual(200, result.StatusCode);

            // Check if the model is added to the database
            var addedModel = await _dbContext.ContactMomentDetails.FirstOrDefaultAsync();
            Assert.IsNotNull(addedModel);
            Assert.AreEqual(validModel.Id, addedModel.Id);
        }



        // Unable to test with a In-Memory database. a In-Memory one doesnt behave as a "reallife" database.
        // throws an unexpected error when trying to save an entity with a duplicate key. therefore we are unable to test if the it updates correctly or not.

        [TestMethod]
        public async Task Post_ModelWithDuplicateId_ReturnsOk()
        {
            Assert.Inconclusive("Test Inconclusive: Unable to test with a In-Memory database.");

            using var dbContext = new BeheerDbContext(_dbContextOptions);

            // Arrange
            var controller = new WriteContactmomentenDetails(dbContext);

            var model1 = new ContactmomentDetails
            {
                Id = "1",
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 1",
                Vraag = "Question 1",
                EmailadresKcm = "test@example.com"
            };

            var model2 = new ContactmomentDetails
            {
                Id = "1", // Duplicate ID
                Startdatum = DateTime.Now,
                Einddatum = DateTime.Now.AddHours(1),
                Gespreksresultaat = "Result 2",
                Vraag = "Question 2",
                EmailadresKcm = "test@example.com"
            };

            // Act
            await controller.Post(model1, CancellationToken.None);
            dbContext.Entry(model1).State = EntityState.Detached;

            var result = await controller.Post(model2, CancellationToken.None) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var updatedModel = await dbContext.ContactMomentDetails.FirstOrDefaultAsync();
            Assert.IsNotNull(updatedModel);
            Assert.AreEqual(model2.Gespreksresultaat, updatedModel.Gespreksresultaat);
        }
    }
}



