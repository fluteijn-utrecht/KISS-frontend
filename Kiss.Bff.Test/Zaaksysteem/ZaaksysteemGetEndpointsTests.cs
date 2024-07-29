using System.IO;
using System.Net;
using System.Text.Json.Nodes;
using IdentityModel;
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
    public class ZaaksysteemGetEndpointsTests
    {
        [TestMethod]
        public async Task Get_endpoint_sets_zaaksysteemId_for_root_level_array_response()
        {
            var result = await RunHappyFlowTest("http://example.com", "my-path", default, "[{}]");

            Assert.IsInstanceOfType<OkObjectResult>(result, out var okObjectResult);
            Assert.IsInstanceOfType<IEnumerable<JsonNode>>(okObjectResult.Value, out var nodes);
            var list = nodes.ToList();
            Assert.AreEqual(1, list.Count);
            var node = list[0];
            Assert.AreEqual("http://example.com/", node["zaaksysteemId"]?.ToString());
        }

        [TestMethod]
        public async Task Get_endpoint_sets_zaaksysteemId_for_paginated_response()
        {
            var result = await RunHappyFlowTest("http://example.com", "my-path", default, "{\"results\": [{}]}");

            Assert.IsInstanceOfType<OkObjectResult>(result, out var okObjectResult);
            Assert.IsInstanceOfType<JsonNode>(okObjectResult.Value, out var rootNode);
            Assert.IsInstanceOfType<JsonArray>(rootNode["results"], out var nodes);
            var list = nodes.ToList();
            Assert.AreEqual(1, list.Count);
            var node = list[0];
            Assert.AreEqual("http://example.com/", node?["zaaksysteemId"]?.ToString());
        }

        [TestMethod]
        public async Task Get_endpoint_sets_zaaksysteemId_for_single_response()
        {
            var id = Guid.NewGuid();
            var result = await RunHappyFlowTest("http://example.com", id.ToString(), default, "{}");

            Assert.IsInstanceOfType<ObjectResult>(result, out var objectResult);
            Assert.AreEqual(200, objectResult.StatusCode);
            Assert.IsInstanceOfType<JsonNode>(objectResult.Value, out var node);
            Assert.AreEqual("http://example.com/", node?["zaaksysteemId"]?.ToString());
        }

        [TestMethod]
        public async Task Get_endpoint_ordering_parameter_is_used_for_sorting()
        {
            var result = await RunHappyFlowTest("http://example.com", "my-path", "my-column", @"{""results"": [
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
        public async Task Get_endpoint_ordering_parameter_is_used_for_sorting_descending()
        {
            var result = await RunHappyFlowTest("http://example.com", "my-path", "-my-column", @"{""results"": [
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
        public async Task Test_bad_request_multiple()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var path = "my-path";

            handler.Expect($"{baseUrl.AsSpan().TrimEnd('/')}/{path.AsSpan().TrimStart('/')}")
                .Respond(HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null);


            var sut = new ZaaksysteemGetEndpoints(new[] { config }, clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(400, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_bad_request_single()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var path = "my-path/" + Guid.NewGuid();

            handler.Expect($"{baseUrl.AsSpan().TrimEnd('/')}/{path.AsSpan().TrimStart('/')}")
                .Respond(HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null);


            var sut = new ZaaksysteemGetEndpoints(new[] { config }, clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(400, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_not_json_single()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var path = "my-path/" + Guid.NewGuid();

            handler.Expect($"{baseUrl.AsSpan().TrimEnd('/')}/{path.AsSpan().TrimStart('/')}")
                .Respond("text/html", "<p>Hello world</p>");
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null);


            var sut = new ZaaksysteemGetEndpoints(new[] { config }, clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(200, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_no_connection_single()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var path = "my-path/" + Guid.NewGuid();

            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null);


            var sut = new ZaaksysteemGetEndpoints(new[] { config }, clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(502, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_no_connection_multiple()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var baseUrl = "http://example.com";
            var path = "my-path";

            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null);


            var sut = new ZaaksysteemGetEndpoints(new[] { config }, clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(502, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_no_config_single()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var path = "my-path/" + Guid.NewGuid();

            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);


            var sut = new ZaaksysteemGetEndpoints(Enumerable.Empty<ZaaksysteemConfig>(), clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(400, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_no_config_multiple()
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            var path = "my-path";

            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);


            var sut = new ZaaksysteemGetEndpoints(Enumerable.Empty<ZaaksysteemConfig>(), clientFactory, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(400, statusCodeResult.StatusCode);
        }

        [TestMethod]
        public async Task Test_pagination_mismatch()
        {
            var config1 = new ZaaksysteemConfig("http://example1.com", "ClientId", "Secret of at least X characters", null, null);
            var config2 = new ZaaksysteemConfig("http://example2.com", "ClientId", "Secret of at least X characters", null, null);
            var path = "my-path";
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            
            // root level array
            handler.Expect($"{config1.BaseUrl}/{path}")
                .Respond("application/json", "[{}]");
            
            // gepagineerd
            handler.Expect($"{config2.BaseUrl}/{path}")
                .Respond("application/json", "{\"results\": [{}]}");

            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler));

            var sut = new ZaaksysteemGetEndpoints(new[] { config1, config2 }, clientFactoryMock.Object, logger);

            var result = await sut.Get(path, null, Enumerable.Empty<string>(), default);
            Assert.IsInstanceOfType<IStatusCodeActionResult>(result, out var statusCodeResult);
            Assert.AreEqual(502, statusCodeResult.StatusCode);
        }

        private static async Task<IActionResult> RunHappyFlowTest(string baseUrl, string path, StringValues ordering, string responseBody)
        {
            var logger = Mock.Of<ILogger<ZaaksysteemGetEndpoints>>();
            var handler = new MockHttpMessageHandler();
            handler.Expect($"{baseUrl.AsSpan().TrimEnd('/')}/{path.AsSpan().TrimStart('/')}")
                .Respond("application/json", responseBody);
            var httpClient = new HttpClient(handler);
            var clientFactory = Mock.Of<IHttpClientFactory>(x => x.CreateClient(It.IsAny<string>()) == httpClient);
            var config = new ZaaksysteemConfig(baseUrl, "ClientId", "Secret of at least X characters", null, null);


            var sut = new ZaaksysteemGetEndpoints(new[] { config }, clientFactory, logger);

            var result = await sut.Get(path, null, ordering, default);

            return result;
        }
    }
}
