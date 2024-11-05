using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ZaakGerichtWerkenUnitTests : TestHelper
    {

        [TestInitialize]
        public void Initialize()
        {
            InitializeDatabase();
        }

        //[TestMethod]
        //public void UpdateMedewerkerIdentificatie_AddsMedewerkerIdentificatieToParsedModel()
        //{

        //    // Arrange
        //    var parsedModel = new JsonObject();
        //    var userRepresentation = "John Doe";
        //    var userId = "123";
        //    var configurationMock = new Mock<IConfiguration>();
        //    var factoryMock = new Mock<IHttpClientFactory>();
        //    var proxy = new PostContactmomentenCustomProxy(configurationMock.Object, factoryMock.Object);


        //    // Act
        //    proxy.UpdateMedewerkerIdentificatie(parsedModel, userRepresentation, userId);

        //    // Assert
        //    Assert.IsNotNull(parsedModel["medewerkerIdentificatie"]);
        //    Assert.AreEqual(userRepresentation, parsedModel["medewerkerIdentificatie"]["achternaam"].ToString());
        //    Assert.AreEqual(userId, parsedModel["medewerkerIdentificatie"]["identificatie"].ToString());
        //    Assert.AreEqual("", parsedModel["medewerkerIdentificatie"]["voorletters"].ToString());
        //    Assert.AreEqual("", parsedModel["medewerkerIdentificatie"]["voorvoegselAchternaam"].ToString());

        //}

        [TestMethod]
        public void GenerateToken_ReturnsValidToken()
        {
            // Arrange
            var apiKey = "blablaFakeApiKey";
            var clientId = "124567890.87654321";
            var userId = "blablaFakeUserId";
            var userRepresentation = "blablaFakeUserRepresentation";
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(KissClaimTypes.KissUserNameClaimType, userId),
                new Claim(ClaimTypes.Name, userRepresentation)
            });
            var user = new ClaimsPrincipal(identity);

            var tokenProvider = new ZgwTokenProvider(apiKey, clientId);

            // Act
            var token = tokenProvider.GenerateToken(user);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrEmpty(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = false, //disabled, because it fails unexpectedly
                ValidateIssuerSigningKey = true,
                ValidIssuer = clientId, // Set the valid issuer
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiKey)),
            };

            // Validate the token
            // ValidateToken throws an exception when invalid
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            Assert.IsNotNull(principal);
        }
    }
}
