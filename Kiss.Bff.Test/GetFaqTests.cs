using Microsoft.AspNetCore.Mvc.Testing;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetFaqTests
    {
        private static WebApplicationFactory<Program> s_factory = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            s_factory = new CustomWebApplicationFactory();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            s_factory.Dispose();
        }

        [TestMethod]
        public async Task GetFaqIsProtectedByLogin()
        {
            using var client = s_factory.CreateDefaultClient();
            using var response = await client.GetAsync("/api/faq");
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
