using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kiss.Bff.Test.Zaaksysteem
{
    [TestClass]
    public class DownloadDocumentTests
    {
        [TestMethod]
        public void Download_returns_correct_result()
        {
            var baseUrl = "http://example.com";
            var key = "een sleutel van minimaal 16 karakters";
            var config = new ZaaksysteemConfig(baseUrl, "", key, null, null);
            var sut = new DownloadDocument(new[] { config }, Mock.Of<ILogger<DownloadDocument>>());

            var id = Guid.NewGuid();
            var result = sut.Download(id, baseUrl);

            Assert.IsInstanceOfType<ProxyResult>(result, out var proxyResult);
            var message = proxyResult.RequestFactory();
            Assert.IsNotNull(message);
            Assert.AreEqual(HttpMethod.Get, message.Method);
        }

        [TestMethod]
        public void Download_returns_500_if_config_is_missing()
        {
            var sut = new DownloadDocument(Enumerable.Empty<ZaaksysteemConfig>(), Mock.Of<ILogger<DownloadDocument>>());
            var result = sut.Download(Guid.NewGuid(), "");
            Assert.IsInstanceOfType<ObjectResult>(result, out var objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.IsInstanceOfType<ProblemDetails>(objectResult.Value, out var problemDetails);
            Assert.AreEqual("Configuratieprobleem", problemDetails.Title);
        }
    }
}
