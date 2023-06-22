using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Controllers;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Kiss.Bff.Beheer.Managementinfo;
using Kiss.Bff.ZaakGerichtWerken;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
        [TestMethod]
        public async Task Post_AddsOrUpdatesContactmomentToDatabase()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var model = new ContactmomentManagementinfoLog
            {
                Id = "1",
                Startdatum = DateTimeOffset.Now,
                Einddatum = DateTimeOffset.Now.AddDays(1),
                Resultaat = "Success",
                PrimaireVraagWeergave = "Question",
                AfwijkendOnderwerp = "Topic",
                EmailadresKcm = "test@example.com"
            };
            var dbContextMock = new Mock<BeheerDbContext>(_dbContextOptions) { CallBase = true };
            dbContextMock.Setup(db => db.SaveChangesAsync(cancellationToken)).Returns(Task.FromResult(1));

            var userClaims = new[]
            {
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
            var controller = new UpsertContactmomentenManagementinfoLog(dbContextMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = user
                    }
                }
            };


            // Act
            var result = await controller.Post(model, cancellationToken);

            // Assert
            dbContextMock.Verify(db => db.AddAsync(model, cancellationToken), Times.Once);
            dbContextMock.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void GenerateToken_ReturnsValidToken()
        {
            // Arrange
            var apiKey = "blablaFakeApiKey";
            var clientId = "124567890.87654321";
            var userId = "blablaFakeUserId";
            var userRepresentation = "blablaFakeUserRepresentation";

            var tokenProvider = new ZgwTokenProvider(apiKey, clientId);

            // Act
            var token = tokenProvider.GenerateToken(userId, userRepresentation);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = clientId, // Set the valid issuer
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey)),
                ClockSkew = TimeSpan.Zero
            };

            // Validate the token
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            Assert.IsNotNull(principal);
        }

        [TestMethod]
        public void UpdateMedewerkerIdentificatie_AddsMedewerkerIdentificatieToParsedModel()
        {

            // Arrange
            var parsedModel = new JsonObject();
            var userRepresentation = "John Doe";
            var userId = "123";
            var configurationMock = new Mock<IConfiguration>();
            var factoryMock = new Mock<IHttpClientFactory>();
            var proxy = new PostContactmomentenCustomProxy(configurationMock.Object, factoryMock.Object);


            // Act
            proxy.UpdateMedewerkerIdentificatie(parsedModel, userRepresentation, userId);

            // Assert
            Assert.IsNotNull(parsedModel["medewerkerIdentificatie"]);
            Assert.AreEqual(userRepresentation, parsedModel["medewerkerIdentificatie"]["achternaam"].ToString());
            Assert.AreEqual(userId, parsedModel["medewerkerIdentificatie"]["identificatie"].ToString());
            Assert.AreEqual("", parsedModel["medewerkerIdentificatie"]["voorletters"].ToString());
            Assert.AreEqual("", parsedModel["medewerkerIdentificatie"]["voorvoegselAchternaam"].ToString());

        }

        [TestMethod]
        public async Task GetGespreksresultaten_ReturnsOkResultWithGespreksresultaten()
        {
            // Arrange
            var gespreksresultaten = new List<Gespreksresultaat>
            {
                new Gespreksresultaat { Id = Guid.NewGuid(), Definitie = "Resultaat 1" },
                new Gespreksresultaat { Id = Guid.NewGuid(), Definitie = "Resultaat 2" }
            };

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                await context.Gespreksresultaten.AddRangeAsync(gespreksresultaten);
                await context.SaveChangesAsync();
            }

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = controller.GetGespreksresultaten();

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

                var okResult = (OkObjectResult)result.Result;
                Assert.IsNotNull(okResult.Value);

                var gespreksresultaatModels = (IAsyncEnumerable<GespreksresultaatModel>)okResult.Value;


                var count = 0;
                await foreach (var item in gespreksresultaatModels)
                {
                    count++;
                    if (count == 1)
                    {
                        Assert.AreEqual("Resultaat 1", item.Definitie);
                    }
                    else if (count == 2)
                    {
                        Assert.AreEqual("Resultaat 2", item.Definitie);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetGespreksresultaat_ReturnsGespreksresultaat()
        {
            // Arrange
            var gespreksresultaatId = Guid.NewGuid();
            var gespreksresultaat = new Gespreksresultaat { Id = gespreksresultaatId, Definitie = "Resultaat" };

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                await context.Gespreksresultaten.AddAsync(gespreksresultaat);
                await context.SaveChangesAsync();
            }

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.GetGespreksresultaat(gespreksresultaatId, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ActionResult<GespreksresultaatModel>));

               // var actionResult = result;
                Assert.IsNotNull(result.Value);

                Assert.AreEqual(gespreksresultaat.Id, result.Value.Id);
                Assert.AreEqual(gespreksresultaat.Definitie, result.Value.Definitie);
            }
        }

        [TestMethod]
        public async Task GetGespreksresultaat_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.GetGespreksresultaat(invalidId, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            }
        }

        [TestMethod]
        public async Task PutGespreksresultaat_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var gespreksresultaatId = Guid.NewGuid();
            var gespreksresultaat = new Gespreksresultaat { Id = gespreksresultaatId, Definitie = "Resultaat" };
            var updatedDefinitie = "Updated Resultaat";

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                await context.Gespreksresultaten.AddAsync(gespreksresultaat);
                await context.SaveChangesAsync();
            }

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.PutGespreksresultaat(gespreksresultaatId, new GespreksresultaatModel(gespreksresultaatId, updatedDefinitie), CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NoContentResult));

                using (var updatedContext = new BeheerDbContext(_dbContextOptions))
                {
                    var updatedGespreksresultaat = await updatedContext.Gespreksresultaten.FindAsync(gespreksresultaatId);
                    Assert.AreEqual(updatedDefinitie, updatedGespreksresultaat.Definitie);
                }
            }
        }

        [TestMethod]
        public async Task PutGespreksresultaat_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.PutGespreksresultaat(invalidId, new GespreksresultaatModel(invalidId, "Resultaat"), CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            }
        }

        [TestMethod]
        public async Task PostGespreksresultaat_ReturnsCreatedResponseWithGespreksresultaat()
        {
            // Arrange
            var gespreksresultaatModel = new GespreksresultaatModel(Guid.NewGuid(), "Resultaat");

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.PostGespreksresultaat(gespreksresultaatModel, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));

                var createdAtActionResult = (CreatedAtActionResult)result.Result;
                Assert.AreEqual(201, createdAtActionResult.StatusCode);

                var gespreksresultaatResult = (GespreksresultaatModel)createdAtActionResult.Value;
                Assert.IsNotNull(gespreksresultaatResult);
                Assert.AreEqual(gespreksresultaatModel.Definitie, gespreksresultaatResult.Definitie);
                Assert.AreEqual(gespreksresultaatResult.Id, gespreksresultaatResult.Id);
            }
        }

        [TestMethod]
        public async Task DeleteGespreksresultaat_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var gespreksresultaatId = Guid.NewGuid();
            var gespreksresultaat = new Gespreksresultaat { Id = gespreksresultaatId, Definitie = "Resultaat" };

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                await context.Gespreksresultaten.AddAsync(gespreksresultaat);
                await context.SaveChangesAsync();
            }

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.DeleteGespreksresultaat(gespreksresultaatId, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NoContentResult));

                using (var updatedContext = new BeheerDbContext(_dbContextOptions))
                {
                    var deletedGespreksresultaat = await updatedContext.Gespreksresultaten.FindAsync(gespreksresultaatId);
                    Assert.IsNull(deletedGespreksresultaat);
                }
            }
        }

        [TestMethod]
        public async Task DeleteGespreksresultaat_WithInvalidId_ReturnsNoContent()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            using (var context = new BeheerDbContext(_dbContextOptions))
            {
                var controller = new GespreksresultatenController(context);

                // Act
                var result = await controller.DeleteGespreksresultaat(invalidId, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NoContentResult));
            }
        }
    }
}
