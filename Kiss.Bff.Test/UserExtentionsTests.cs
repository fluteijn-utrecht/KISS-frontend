using Duende.IdentityModel;
using System.Security.Claims;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class UserExtensionsTests
    {
        [TestMethod]
        public void GetId_UserHasObjectIdentifier_ReturnsObjectIdentifier()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "12345"),
            }));

            // Act
            var id = user.GetId();

            // Assert
            Assert.AreEqual("12345", id);
        }

        [TestMethod]
        public void GetId_UserHasNameIdentifier_ReturnsNameIdentifier()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "67890"),
            }));

            // Act
            var id = user.GetId();

            // Assert
            Assert.AreEqual("67890", id);
        }

        [TestMethod]
        public void GetEmail_UserHasJwtEmailClaim_ReturnsJwtEmailClaim()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimTypes.Email, "test@example.com"),
            }));

            // Act
            var email = user.GetEmail();

            // Assert
            Assert.AreEqual("test@example.com", email);
        }

        [TestMethod]
        public void GetEmail_UserHasPreferredUserNameClaim_ReturnsPreferredUserNameClaim()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimTypes.PreferredUserName, "user123"),
            }));

            // Act
            var email = user.GetEmail();

            // Assert
            Assert.AreEqual("user123", email);
        }

        [TestMethod]
        public void GetLastName_UserHasJwtFamilyNameClaim_ReturnsJwtFamilyNameClaim()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimTypes.FamilyName, "Doe"),
            }));

            // Act
            var lastName = user.GetLastName();

            // Assert
            Assert.AreEqual("Doe", lastName);
        }

        [TestMethod]
        public void GetLastName_UserHasJwtNameClaim_ReturnsJwtNameClaim()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimTypes.Name, "John Doe"),
            }));

            // Act
            var lastName = user.GetLastName();

            // Assert
            Assert.AreEqual("John Doe", lastName);
        }

        [TestMethod]
        public void GetLastName_UserHasIdentityName_ReturnsIdentityName()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "User1"),
            }));

            // Act
            var lastName = user.GetLastName();

            // Assert
            Assert.AreEqual("User1", lastName);
        }

        [TestMethod]
        public void GetFirstName_UserHasJwtGivenNameClaim_ReturnsJwtGivenNameClaim()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtClaimTypes.GivenName, "John"),
            }));

            // Act
            var firstName = user.GetFirstName();

            // Assert
            Assert.AreEqual("John", firstName);
        }

        [TestMethod]
        public void GetMedewerkerIdentificatie_ReturnsJsonObjectWithTruncatedValues()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(KissClaimTypes.KissUserNameClaimType, "test@example.com"),
                new Claim(JwtClaimTypes.FamilyName, "DoeLastName"),
                new Claim(JwtClaimTypes.GivenName, "JohnFirstName"),
            }));

            // Act
            var medewerkerIdentificatie = user.GetMedewerkerIdentificatie(null);

            // Assert
            Assert.AreEqual("DoeLastName", medewerkerIdentificatie["achternaam"]?.GetValue<string>());
            Assert.AreEqual("test@example.com", medewerkerIdentificatie["identificatie"]?.GetValue<string>());
            Assert.AreEqual("JohnFirstName", medewerkerIdentificatie["voorletters"]?.GetValue<string>());
            Assert.AreEqual("", medewerkerIdentificatie["voorvoegselAchternaam"]?.GetValue<string>());
        }
    }
}
