using System.Security.Claims;
using Kiss.Bff.Beheer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using static Kiss.Bff.Beheer.Verwerking.VerwerkingMiddleware;

namespace Kiss.Bff.Test
{
    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "TestDatabase");
        }
    }

    [TestClass]
    public class UnitTest
    {
        private DbContextOptions<BeheerDbContext> _dbContextOptions;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }
        [TestMethod]
        public async Task SendAsync_LogsHttpRequestToDatabase()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.nu.nl");

            var user = new ClaimsPrincipal();
            var loggerMock = new Mock<ILogger<VerwerkingsHttpClientMiddleware>>();

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var middleware = new VerwerkingsHttpClientMiddleware(context, user, loggerMock.Object);

                // Act
                var response = await middleware.SendAsync(async (r, _) => new HttpResponseMessage(), request, CancellationToken.None);

                // Assert
                Assert.IsTrue(response.IsSuccessStatusCode);

                var loggedRequest = context.VerwerkingsLogs.FirstOrDefault();
                Assert.IsNotNull(loggedRequest);
                Assert.AreEqual(request.Method.Method, loggedRequest.Method);
                Assert.AreEqual(request.RequestUri.AbsoluteUri, loggedRequest.ApiEndpoint);
                // Assert additional properties as needed
            }
        }
    }
}
