using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class GetZaak : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _zakenProxyConfigs;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GetZaak> _logger;

        public GetZaak(
            IEnumerable<ZaaksysteemConfig> zakenProxyConfigs,
            IHttpClientFactory httpClientFactory,
            ILogger<GetZaak> logger)
        {
            _zakenProxyConfigs = zakenProxyConfigs;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("api/zaken/zaken/api/{version}/zaken/{id}")]
        public async Task<IActionResult> Get([FromHeader(Name = "ZaaksysteemId")] string? zaaksysteemId, string version, string id, CancellationToken token)
        {
            var path = $"zaken/api/{version}/zaken/{id}";

            return !string.IsNullOrWhiteSpace(zaaksysteemId)
                ? HandleSingleZaaksysteem(zaaksysteemId, path)
                : await HandleMultipleZaaksystemen(path, token);
        }

        private IActionResult HandleSingleZaaksysteem(string? zaaksysteemId, string path)
        {
            var config = _zakenProxyConfigs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();

            if (config == null)
            {
                _logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}", zaaksysteemId);
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + zaaksysteemId,
                    statusCode: 500
                );
            }

            return new ProxyResult(() =>
            {
                var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.ApplyZaaksysteemHeaders(config, User);
                return message;
            });
        }

        private async Task<IActionResult> HandleMultipleZaaksystemen(string path, CancellationToken token)
        {
            var results = await SendRequestToZaaksystemen(path, token);

            var (successBaseAddress, successMessage) = results.FirstOrDefault(x => x.Message?.IsSuccessStatusCode ?? false);
            
            if (successMessage == null || successBaseAddress == null)
            {
                if (results.All(x => x.Message?.StatusCode == System.Net.HttpStatusCode.NotFound)) return NotFound();
                return StatusCode(502);
            }

            var json = await successMessage.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: token) ?? new();
            json["zaaksysteemId"] = successBaseAddress!.ToString();
            
            return Ok(json);
        }

        private Task<(Uri? BaseAddress, HttpResponseMessage? Message)[]> SendRequestToZaaksystemen(string path, CancellationToken token)
        {
            var tasks = _zakenProxyConfigs
                .Select(async (config) =>
                {
                    var client = _httpClientFactory.CreateClient(config, User);
                    try
                    {
                        var queryString = Request?.QueryString.ToString();
                        var message = await client.GetAsync($"{path}{queryString}", HttpCompletionOption.ResponseHeadersRead, token);
                        Response?.RegisterForDispose(message);
                        return (client.BaseAddress, Message: (HttpResponseMessage?)message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Zaaksysteem met baseUrl {BaseUrl} is niet bereikbaar", client.BaseAddress);
                    }
                    return (client.BaseAddress, null);
                });

            return Task.WhenAll(tasks);
        }
    }
}
