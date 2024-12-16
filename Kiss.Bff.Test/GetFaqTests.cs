using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Faq;
using Kiss.Bff.Intern.ContactmomentDetails.Data.Entities;
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
        public async Task Get_ReturnsTopQuestions()
        {
            using var dbContext = new BeheerDbContext(_dbContextOptions);
            // Arrange
            var controller = new GetFaq(dbContext);

            var testData = new List<ContactmomentDetails>();
            var topQuestionsCount = 10;

            // Add 500 questions
            for (int i = 1; i <= 500; i++)
            {
                testData.Add(new ContactmomentDetails
                {
                    Id = i.ToString(),
                    Vraag = $"Question {i}",
                    Einddatum = DateTime.UtcNow
                });
            }

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
            var expectedQuestions = testData
                .OrderByDescending(x => x.Einddatum)
                .Where(x => !string.IsNullOrWhiteSpace(x.Vraag))
                .GroupBy(x => x.Vraag)
                .OrderByDescending(x => x.Count())
                .Take(topQuestionsCount)
                .Select(x => x.Key);

            CollectionAssert.AreEqual(expectedQuestions.ToList(), resultList.ToList());
        }

    }
}
