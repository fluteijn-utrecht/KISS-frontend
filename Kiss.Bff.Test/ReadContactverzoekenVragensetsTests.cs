using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactverzoeken;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ReadContactverzoekenVragenSetsTests
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
        public async Task Get_ContactVerzoekVragenSetsExist_ReturnsOkWithList()
        {
            // Arrange
            var controller = new ReadContactverzoekenVragenSets(_dbContext);

            var vragenSets = new List<ContactVerzoekVragenSet>
            {
                new ContactVerzoekVragenSet
                {
                    Id = 1,
                    Naam = "VragenSet 1",
                    JsonVragen = "{ \"Question1\": \"Answer1\" }",
                    AfdelingId = "Dept1"
                },
                new ContactVerzoekVragenSet
                {
                    Id = 2,
                    Naam = "VragenSet 2",
                    JsonVragen = "{ \"Question2\": \"Answer2\" }",
                    AfdelingId = "Dept2"
                }
            };

            await _dbContext.ContactVerzoekVragenSets.AddRangeAsync(vragenSets);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await controller.Get(CancellationToken.None) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var resultList = result.Value as List<ContactVerzoekVragenSet>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(2, resultList.Count);
        }

        [TestMethod]
        public async Task Get_NoContactVerzoekVragenSets_ReturnsOkWithEmptyList()
        {
            // Arrange
            var controller = new ReadContactverzoekenVragenSets(_dbContext);

            // Act
            var result = await controller.Get(CancellationToken.None) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var resultList = result.Value as List<ContactVerzoekVragenSet>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(0, resultList.Count);
        }
    }
}
