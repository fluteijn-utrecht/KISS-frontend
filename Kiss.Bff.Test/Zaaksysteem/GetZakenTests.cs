using System.Net;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using RichardSzalay.MockHttp;

namespace Kiss.Bff.Test.Zaaksysteem
{
    [TestClass]
    public class GetZakenTests
    {
        [TestMethod]
        public async Task GetZaken_sets_zaaksysteemId()
        {
            var result = await RunHappyFlowTest("http://example.com", default, "{\"results\": [{}]}");

            Assert.IsInstanceOfType<OkObjectResult>(result, out var okObjectResult);
            Assert.IsInstanceOfType<JsonNode>(okObjectResult.Value, out var rootNode);
            Assert.IsInstanceOfType<JsonArray>(rootNode["results"], out var nodes);
            var list = nodes.ToList();
            Assert.AreEqual(1, list.Count);
            var node = list[0];
            Assert.AreEqual("http://example.com/", node?["zaaksysteemId"]?.ToString());
        }

        [TestMethod]
        public async Task GetZaken_uses_ordering_parameter_for_sorting_ascending()
        {
            var result = await RunHappyFlowTest("http://example.com", "my-column", @"{""results"": [
                {""my-column"": 2}, {""my-column"": 1}, {""my-column"": 3}
            ]}");

            Assert.IsInstanceOfType<OkObjectResult>(result, out var okObjectResult);
            Assert.IsInstanceOfType<JsonNode>(okObjectResult.Value, out var rootNode);
            Assert.IsInstanceOfType<JsonArray>(rootNode["results"], out var nodes);
            var list = nodes.Select(x => x!["my-column"]!.GetValue<int>()).ToArray();
            Assert.AreEqual(3, list.Length);
            Assert.AreEqual(list[0], 1);
            Assert.AreEqual(list[1], 2);
            Assert.AreEqual(list[2], 3);
        }

        [TestMethod]
        public async Task GetZaken_uses_ordering_parameter_for_sorting_descending()
        {
            var result = await RunHappyFlowTest("http://example.com", "-my-column", @"{""results"": [
                {""my-column"": 2}, {""my-column"": 1}, {""my-column"": 3}
            ]}");

            Assert.IsInstanceOfType<OkObjectResult>(result, out var okObjectResult);
            Assert.IsInstanceOfType<JsonNode>(okObjectResult.Value, out var rootNode);
            Assert.IsInstanceOfType<JsonArray>(rootNode["results"], out var nodes);
            var list = nodes.Select(x => x!["my-column"]!.GetValue<int>()).ToArray();
            Assert.AreEqual(3, list.Length);
            Assert.AreEqual(list[0], 3);
            Assert.AreEqual(list[1], 2);
            Assert.AreEqual(list[2], 1);
        }

        [TestMethod]
        public async Task GetZaken_correctly_proxies_400_status_code()
        {
            var logger = Mock.Of<ILogger<GetZaken>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var version = "my-version";

            handler.Expect($"{baseUrl}/zaken/api/{version}/zaken")
                .Respond(HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null, null);


            var sut = new GetZaken(new[] { config }, clientFactory, logger);

            var result = await sut.Get(version, Enumerable.Empty<string>(), null, null, default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(400, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task GetZaken_correctly_proxies_unreachable_host()
        {
            var logger = Mock.Of<ILogger<GetZaken>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var version = "my-version";

            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null, null);


            var sut = new GetZaken(new[] { config }, clientFactory, logger);

            var result = await sut.Get(version, Enumerable.Empty<string>(), null, null, default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(502, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task GetZaken_correctly_handles_missing_config()
        {
            var logger = Mock.Of<ILogger<GetZaken>>();
            var handler = new MockHttpMessageHandler();
            var version = "my-version";

            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);


            var sut = new GetZaken(Enumerable.Empty<ZaaksysteemConfig>(), clientFactory, logger);

            var result = await sut.Get(version, Enumerable.Empty<string>(), null, null, default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task GetZaken_with_kvknummer_and_rsin_queries_by_rsin_by_default()
        {
            var logger = Mock.Of<ILogger<GetZaken>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var version = "my-version";
            var rsin = "12345";
            var kvkNummer = "54321";

            handler.Expect($"{baseUrl}/zaken/api/{version}/zaken?rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId={rsin}")
                .Respond("application/json", "{\"results\": []}");
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null, null);


            var sut = new GetZaken(new[] { config }, clientFactory, logger);

            var result = await sut.Get(version, Enumerable.Empty<string>(), kvkNummer, rsin, default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(200, statusCodeResult.StatusCode);
            handler.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task GetZaken_with_kvknummer_and_rsin_queries_by_kvknummer_if_specified_in_config()
        {
            var logger = Mock.Of<ILogger<GetZaken>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var version = "my-version";
            var rsin = "12345";
            var kvkNummer = "54321";

            handler.Expect($"{baseUrl}/zaken/api/{version}/zaken?rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId={kvkNummer}")
                .Respond("application/json", "{\"results\": []}");
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null, "kvkNummer");


            var sut = new GetZaken(new[] { config }, clientFactory, logger);

            var result = await sut.Get(version, Enumerable.Empty<string>(), kvkNummer, rsin, default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(200, statusCodeResult.StatusCode);
            handler.VerifyNoOutstandingExpectation();
        }

        private static async Task<IActionResult> RunHappyFlowTest(string baseUrl, StringValues ordering, string responseBody)
        {
            const string Version = "my-version";
            var logger = Mock.Of<ILogger<GetZaken>>();
            var handler = new MockHttpMessageHandler();
            handler.Expect($"{baseUrl}/zaken/api/{Version}/zaken")
                .Respond("application/json", responseBody);
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null, null);


            var sut = new GetZaken(new[] { config }, clientFactory, logger);

            var result = await sut.Get(Version, ordering, null, null, default);

            return result;
        }
    }
}
