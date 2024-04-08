using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Intern.Kanalen;
using Kiss.Bff.Intern.Kanalen.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class KanalenTests : TestHelper
    {


        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }


        [TestMethod]
        public async Task Get_WithValidId_ReturnsKanaal()
        {
            // Arrange
            var item = new Kanaal { Naam = "testkanaal" };

            using var context = new BeheerDbContext(_dbContextOptions);
            context.Kanalen.RemoveRange(context.Kanalen);
            await context.Kanalen.AddAsync(item);
            await context.SaveChangesAsync();

            var feature = new KanaalBeheerDetails(context);

            // Act
            var result = await feature.Get(item.Id);

            // Assert
            Assert.IsInstanceOfType(result.Value, typeof(KanaalOverzichtModel));

            var returnedItem = result?.Value;
            Assert.AreEqual(item.Id, returnedItem?.Id);
            Assert.AreEqual(item.Naam, returnedItem?.Naam);

        }

        [TestMethod]
        public async Task Get_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            using var context = new BeheerDbContext(_dbContextOptions);
            var feature = new KanaalBeheerDetails(context);

            // Act
            var result = await feature.Get(Guid.NewGuid());

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }



        [TestMethod]
        public async Task Put_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var item = new Kanaal { Naam = "testkanaal" };

            using var context = new BeheerDbContext(_dbContextOptions);
            context.Kanalen.RemoveRange(context.Kanalen);
            await context.Kanalen.AddAsync(item);
            await context.SaveChangesAsync();


            var updatemodel = new KanaalBewerkenModel(item.Id, "Updated");


            var feature = new KanaalBewerken(context);

            // Act
            var result = await feature.Put(item.Id, updatemodel, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var updatedEntity = await context.Kanalen.FindAsync(item.Id);
            Assert.IsNotNull(updatedEntity);
            Assert.AreEqual(updatemodel.Naam, updatedEntity.Naam);


        }

        [TestMethod]
        public async Task PutLink_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var item = new Kanaal { Naam = "testkanaal" };

            using var context = new BeheerDbContext(_dbContextOptions);
            context.Kanalen.RemoveRange(context.Kanalen);
            await context.Kanalen.AddAsync(item);
            await context.SaveChangesAsync();

            var id = Guid.NewGuid();
            var updatemodel = new KanaalBewerkenModel(id, "Updated");

            var feature = new KanaalBewerken(context);

            // Act
            var result = await feature.Put(id, updatemodel, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task Post_WithValid_ReturnsCreatedResult()
        {
            // Arrange
            using var context = new BeheerDbContext(_dbContextOptions);
            context.Kanalen.RemoveRange(context.Kanalen);

            var postmodel = new KanaalToevoegenModel("testkanaal");

            var feature = new KanaalToevoegen(context);

            // Act
            var result = feature.Post(postmodel, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ActionResult<KanaalToevoegenModel>));

        }

        [TestMethod]
        public async Task DeleteLink_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var item = new Kanaal { Naam = "testkanaal" };

            using var context = new BeheerDbContext(_dbContextOptions);
            context.Kanalen.RemoveRange(context.Kanalen);
            await context.Kanalen.AddAsync(item);
            await context.SaveChangesAsync();

            var feature = new KanaalVerwijderen(context);

            // Act
            var result = await feature.Delete(item.Id, CancellationToken.None);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var deletedLink = await context.Kanalen.FindAsync(item.Id);
            Assert.IsNull(deletedLink);
        }

        [TestMethod]
        public async Task DeleteLink_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            using var context = new BeheerDbContext(_dbContextOptions);
            var feature = new KanaalVerwijderen(context);

            // Act
            var result = await feature.Delete(Guid.NewGuid(), CancellationToken.None);


            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
