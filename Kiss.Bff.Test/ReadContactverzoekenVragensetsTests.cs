using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Intern.ContactverzoekenVragensets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ReadContactverzoekenVragenSetsTests : TestHelper
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
        public async Task Get_ContactVerzoekVragenSetsExist_ReturnsOkWithList()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new ReadContactverzoekenVragenSets(dbContext);

            var vragenSets = new List<ContactVerzoekVragenSet>
            {
                new ContactVerzoekVragenSet
                {
                    Id = 1,
                    Titel = "VragenSet 1",
                    JsonVragen = "{ \"Question1\": \"Answer1\" }",
                    OrganisatorischeEenheidId = "Dept1",
                    OrganisatorischeEenheidNaam = "Department 1" ,
                    OrganisatorischeEenheidSoort = "afdeling"
                    
                },
                new ContactVerzoekVragenSet
                {
                    Id = 2,
                    Titel = "VragenSet 2",
                    JsonVragen = "{ \"Question2\": \"Answer2\" }",
                    OrganisatorischeEenheidId = "Dept2",
                    OrganisatorischeEenheidNaam = "Department 2" ,
                    OrganisatorischeEenheidSoort = "afdeling"
                }
            };

            await dbContext.ContactVerzoekVragenSets.AddRangeAsync(vragenSets);
            await dbContext.SaveChangesAsync();

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
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new ReadContactverzoekenVragenSets(dbContext);

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
