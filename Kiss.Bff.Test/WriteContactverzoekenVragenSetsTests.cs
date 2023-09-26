using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Kiss.Bff.ZaakGerichtWerken.Contactverzoeken;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

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
        public void PostContactVerzoekVragenSet_ReturnsOkResult_WhenModelIsAdded()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var model = new ContactVerzoekVragenSet { Titel = "Test Titel" };

            // Act
            var result = controller.Post(model, new CancellationToken()).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(1, dbContext.ContactVerzoekVragenSets.Count());
            Assert.AreEqual("Test Titel", dbContext.ContactVerzoekVragenSets.First().Titel);
        }

        [TestMethod]
        public void PutContactVerzoekVragenSet_ReturnsOkResult_WhenModelIsUpdated()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var originalModel = new ContactVerzoekVragenSet { Titel = "Originele Titel" };
            dbContext.ContactVerzoekVragenSets.Add(originalModel);
            dbContext.SaveChanges();

            var updatedModel = new ContactVerzoekVragenSet { Titel = "Geupdaten Titel" };

            // Act
            var result = controller.Put(originalModel.Id, updatedModel, new CancellationToken()).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual("Geupdaten Titel", dbContext.ContactVerzoekVragenSets.First().Titel);
        }

        [TestMethod]
        public void DeleteContactVerzoekVragenSet_ReturnsOkResult_WhenModelIsDeleted()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var model = new ContactVerzoekVragenSet { Titel = "Test Titel" };
            dbContext.ContactVerzoekVragenSets.Add(model);
            dbContext.SaveChanges();

            // Act
            var result = controller.Delete(model.Id, new CancellationToken()).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(0, dbContext.ContactVerzoekVragenSets.Count());
        }

        [TestMethod]
        public void PutContactVerzoekVragenSet_ReturnsNotFound_WhenModelDoesNotExist()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);
            var model = new ContactVerzoekVragenSet { Titel = "Niet bestaande Title" };

            // Act
            var result = controller.Put(1, model, new CancellationToken()).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void DeleteContactVerzoekVragenSet_ReturnsNotFound_WhenModelDoesNotExist()
        {
            // Arrange
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            var controller = new WriteContactverzoekenVragenSets(dbContext);

            // Act
            var result = controller.Delete(1, new CancellationToken()).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
