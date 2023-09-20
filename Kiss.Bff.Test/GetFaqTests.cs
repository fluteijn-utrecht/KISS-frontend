using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Faq;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetFaqTests : TestHelper
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
        public void Get_ReturnsTopQuestions()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new GetFaq(dbContext);

            var testData = new List<ContactmomentDetails>
            {
                new ContactmomentDetails
                {
                    Id = "1",
                    Vraag = "Question 1",
                    Einddatum = DateTime.UtcNow
                },
                new ContactmomentDetails
                {
                    Id = "2",
                    Vraag = "Question 2",
                    Einddatum = DateTime.UtcNow
                },
                new ContactmomentDetails
                {
                    Id = "3",
                    Vraag = "Question 1",
                    Einddatum = DateTime.UtcNow
                },
            };

            dbContext.ContactMomentDetails.AddRange(testData);
            dbContext.SaveChanges();

            // Act
            var result = controller.Get() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var resultList = result.Value as IEnumerable<string>;
            Assert.IsNotNull(resultList);

            // Ensure that the result contains the top questions
            var expectedQuestions = new List<string> { "Question 1", "Question 2" };
            CollectionAssert.AreEqual(expectedQuestions, resultList.ToList());
        }
    }
}
