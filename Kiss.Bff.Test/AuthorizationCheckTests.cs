using Microsoft.AspNetCore.Mvc.Testing;
using static System.Net.HttpStatusCode;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class AuthorizationCheckTests
    {
        private static WebApplicationFactory<Program> s_factory = null!;
        private static HttpClient s_client = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            s_factory = new CustomWebApplicationFactory();
            s_client = s_factory.CreateDefaultClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            s_client?.Dispose();
            s_factory?.Dispose();
        }

        [DataTestMethod]
        [DataRow("/api/postcontactmomenten", "post")]
        [DataRow("/api/internetaak/api/version/objects", "post")]
        [DataRow("/api/faq")]
        public async Task Test(string url, string method = "get")
        {
            using var request = new HttpRequestMessage(new(method), url);
            using var response = await s_client.SendAsync(request);
            Assert.AreEqual(Unauthorized, response.StatusCode);
        }
    }
}
