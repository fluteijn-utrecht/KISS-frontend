using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kiss.Bff.Test.Zaaksysteem
{
    [TestClass]
    public class GetZaaksysteemDeeplinkConfigTest
    {
        [TestMethod]
        public void GetConfigReturnsCorrectVariables()
        {
            var baseUrl = "http://example.com";
            var deeplinkUrl = Guid.NewGuid().ToString();
            var idProperty = Guid.NewGuid().ToString();

            var config = new ZaaksysteemConfig(baseUrl, "", "", deeplinkUrl, idProperty, null);

            var sut = new GetZaaksysteemDeeplinkConfig(new[] { config }, Mock.Of<ILogger<GetZaaksysteemDeeplinkConfig>>());
            var result = sut.Get(baseUrl);

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var obj = (result as ObjectResult)!.Value;
            Assert.IsInstanceOfType(obj, typeof(ZaaksysteemDeeplinkConfig));
            var deeplinkConfig = obj as ZaaksysteemDeeplinkConfig;
            Assert.AreEqual(deeplinkUrl, deeplinkConfig!.BaseUrl);
            Assert.AreEqual(idProperty, deeplinkConfig!.IdProperty);
        }

        [TestMethod]
        public void Get_config_returns_500_if_config_is_missing()
        {
            var sut = new GetZaaksysteemDeeplinkConfig(Enumerable.Empty<ZaaksysteemConfig>(), Mock.Of<ILogger<GetZaaksysteemDeeplinkConfig>>());
            var result = sut.Get("");
            Assert.IsInstanceOfType<ObjectResult>(result, out var objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.IsInstanceOfType<ProblemDetails>(objectResult.Value, out var problemDetails);
            Assert.AreEqual("Configuratieprobleem", problemDetails.Title);
        }
    }
}
