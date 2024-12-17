using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Kiss.Bff.Beheer.Data;
using System.Linq;
using System.Threading;
using Kiss.Bff.Intern.ContactverzoekenVragensets;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class WriteContactverzoekenVragenSetsTests : TestHelper
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }


        [TestMethod]
        public async Task PostContactVerzoekVragenSet_ReturnsOkResult_WhenModelIsAdded()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var model = new ContactVerzoekVragenSet
            {
                Titel = "Test Titel",
                AfdelingId = string.Empty,
                AfdelingNaam = "Test Name"
            };

            // Act
            var result = await controller.Post(model, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(1, dbContext.ContactVerzoekVragenSets.Count());
            Assert.AreEqual("Test Titel", dbContext.ContactVerzoekVragenSets.First().Titel);
        }

        [TestMethod]
        public async Task PutContactVerzoekVragenSet_ReturnsOkResult_WhenModelIsUpdated()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var originalModel = new ContactVerzoekVragenSet
            {
                Titel = "Originele Titel",
                AfdelingId = string.Empty,
                AfdelingNaam = "Original Name"
            };
            dbContext.ContactVerzoekVragenSets.Add(originalModel);
            await dbContext.SaveChangesAsync();

            var updatedModel = new ContactVerzoekVragenSet
            {
                Titel = "Geupdaten Titel",
                AfdelingId = string.Empty,
                AfdelingNaam = "Updated Name"
            };

            // Act
            var result = await controller.Put(originalModel.Id, updatedModel, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual("Geupdaten Titel", dbContext.ContactVerzoekVragenSets.First().Titel);
        }

        [TestMethod]
        public async Task DeleteContactVerzoekVragenSet_ReturnsOkResult_WhenModelIsDeleted()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var model = new ContactVerzoekVragenSet
            {
                Titel = "Test Titel",
                AfdelingId = string.Empty,
                AfdelingNaam = "Test Name"
            };
            dbContext.ContactVerzoekVragenSets.Add(model);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await controller.Delete(model.Id, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(0, dbContext.ContactVerzoekVragenSets.Count());
        }
    }
}
