using System;
using System.Collections.Generic;
using System.Linq;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Faq;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetFaqTests
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
        public void Get_ReturnsTopQuestions()
        {
            // Arrange
            var controller = new GetFaq(_dbContext);

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

            _dbContext.ContactMomentDetails.AddRange(testData);
            _dbContext.SaveChanges();

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
