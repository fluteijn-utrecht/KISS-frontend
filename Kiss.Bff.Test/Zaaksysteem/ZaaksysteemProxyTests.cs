using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kiss.Bff.Test.Zaaksysteem
{
    [TestClass]
    public class ZaaksysteemProxyTests
    {
        [TestMethod]
        public void Proxy_returns_correct_result()
        {
            var baseUrl = "http://example.com";
            var key = "een sleutel van minimaal 16 karakters";
            var config = new ZaaksysteemConfig(baseUrl, "", key, null, null);
            var sut = new ZaaksysteemProxy(new[] { config }, Mock.Of<ILogger<ZaaksysteemProxy>>());

            var id = Guid.NewGuid();
            var result = sut.Get("my-path", baseUrl);

            Assert.IsInstanceOfType<ProxyResult>(result, out var proxyResult);
            var message = proxyResult.RequestFactory();
            Assert.IsNotNull(message);
            Assert.AreEqual(HttpMethod.Get, message.Method);
        }

        [TestMethod]
        public void Proxy_returns_500_if_config_is_missing()
        {
            var sut = new ZaaksysteemProxy(Enumerable.Empty<ZaaksysteemConfig>(), Mock.Of<ILogger<ZaaksysteemProxy>>());
            var result = sut.Get("my-path", "");
            Assert.IsInstanceOfType<ObjectResult>(result, out var objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.IsInstanceOfType<ProblemDetails>(objectResult.Value, out var problemDetails);
            Assert.AreEqual("Configuratieprobleem", problemDetails.Title);
        }
    }
}
