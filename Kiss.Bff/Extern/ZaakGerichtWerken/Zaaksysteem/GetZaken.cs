using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class GetZaken : ControllerBase
    {
        const string KvkNummer = "kvkNummer";
        const string Rsin = "rsin";

        private readonly IEnumerable<ZaaksysteemConfig> _configs;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GetZaken> _logger;

        public GetZaken(
            IEnumerable<ZaaksysteemConfig> zakenProxyConfigs,
            IHttpClientFactory httpClientFactory,
            ILogger<GetZaken> logger)
        {
            _configs = zakenProxyConfigs;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        /// <summary>
        /// Als onbekend is in welk zaaksysteeem de gevraagde gegevens gevonden kunnen worden, zoeken we in alle systemen.
        /// We halen maar 1 resultaatpagina op per bron, aangezien we 0 of 1 record verwachten.
        /// De resultaten van de verschilllende systemen worden samengevoegd en gesorteerd.
        /// Er wordt niet ontdubbeld
        /// Wanneer bekend is in welk zaaksysteem de gegevens te vinden zijn, kunnen we een eenvoudig proxy mechanisme hanteren. zie Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.ZaaksysteemProxy
        /// </summary>
        /// <param name="version"></param>
        /// <param name="ordering"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("api/zaken/zaken/api/{version}/zaken")]
        public async Task<IActionResult> Get(
            [FromRoute] string version,
            [FromQuery(Name = "ordering")] IEnumerable<string> ordering,
            [FromQuery(Name = KvkNummer)] string? kvkNummer,
            [FromQuery(Name = Rsin)] string? rsin,
            CancellationToken token)
        {
            var messages = await SendRequestToZaaksystemen($"zaken/api/{version}/zaken", kvkNummer, rsin, token);

            if (messages.Length == 0)
            {
                return GeenZaaksysteemGeconfigureerdError();
            }

            var success = messages.Where(x => x.Message?.IsSuccessStatusCode ?? false).ToList();

            if (!success.Any())
            {
                var message = messages
                    .Where(x => x.Message != null)
                    .Select(x => x.Message)
                    .FirstOrDefault();

                if (message == null)
                {
                    return GeenZaaksysteemBeschikbaarError();
                }

                return new CopyResponseMessageResult(message);
            }

            var tasks = success.Select(async x =>
            {
                var node = await x.Message!.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: token);
                if (node is JsonObject obj && obj["results"] is JsonArray a)
                    return a.OfType<JsonObject>().Select(n => AddZaaksysteemId(n, x.BaseAddress!.ToString()));

                 // we krijgen geen gepagineerd resultaat terug...
                 // negeren?
                return Enumerable.Empty<JsonObject>();
            });

            var nodes = await Task.WhenAll(tasks);

            // als er zaaksystemen zijn die een foutmelding hebben opgeleverd, geven we dit terug als header
            // zodat we dat evt aan de voorkant kunnen tonen
            var hasErrors = messages.Any(x => x.Message == null || !x.Message.IsSuccessStatusCode);

            if (hasErrors)
            {
                Response?.Headers?.Add("Has-Errors", "True");
            }

            var merged = nodes
                .SelectMany(x=> x)
                // this makes the code below more simple because we start with an IOrderedEnumerable
                .OrderBy(x => true);

            foreach (var item in ordering)
            {
                var descending = item.StartsWith('-');

                var sortColumn = descending
                    ? item.Substring(1)
                    : item;

                string? GetSortValue(JsonObject obj) => obj.TryGetPropertyValue(sortColumn, out var val)
                    ? val?.ToString()
                    : null;

                merged = descending
                    ? merged.ThenByDescending(GetSortValue)
                    : merged.ThenBy(GetSortValue);
            }

            var cloned = merged.Select(x =>
                // we need to clone the nodes because we attach them to a new parent further down.
                // .NET 8 adds a DeepClone method but we're on 6
                JsonSerializer.Deserialize<JsonNode>(x)).ToArray();

            var result = new JsonObject
            {
                ["results"] = new JsonArray(cloned),
                ["count"] = cloned.Length
            };

            return Ok(result);
        }

        private Task<(Uri? BaseAddress, HttpResponseMessage? Message)[]> SendRequestToZaaksystemen(string path, string? kvkNummer, string? rsin, CancellationToken token)
        {
            var tasks = _configs
                .Select(async (config) =>
                {
                    var client = CreateClient(config);
                    try
                    {
                        var queryString = Request?.QueryString.ToString();
                        
                        if (!string.IsNullOrWhiteSpace(kvkNummer) && !string.IsNullOrWhiteSpace(rsin))
                        {
                            var identifier = config.NietNatuurlijkPersoonIdentifier == "kvkNummer"
                                ? kvkNummer
                                : rsin;

                            var queryParametersExcludingRsinAndKvk = Request?.Query?
                                .Where(x => x.Key != KvkNummer && x.Key != Rsin) 
                                ?? Enumerable.Empty<KeyValuePair<string, StringValues>>();

                            var dict = queryParametersExcludingRsinAndKvk
                                .Append(new("rol__betrokkeneIdentificatie__nietNatuurlijkPersoon__innNnpId", identifier))
                                .SelectMany(x=> x.Value.Select(v => $"{x.Key}={x.Value}"));

                            queryString = $"?{string.Join('&', dict)}";
                        }
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

        private static JsonObject AddZaaksysteemId(JsonObject node, string? zaaksysteemId)
        {
            node["zaaksysteemId"] = zaaksysteemId;
            return node;
        }

        private IActionResult GeenZaaksysteemBeschikbaarError()
        {
            const string Message = "Er is geen enkel zaaksysteem bereikbaar";
            _logger.LogError(Message);

            return Problem(
                title: "Configuratieprobleem",
                detail: Message,
                statusCode: 502
            );
        }

        private IActionResult GeenZaaksysteemGeconfigureerdError()
        {
            const string Message = "Er is geen zaaksysteem geconfigureerd";
            _logger.LogError(Message);

            return Problem(
                title: "Configuratieprobleem",
                detail: Message,
                statusCode: 500
            );
        }

        private HttpClient CreateClient(ZaaksysteemConfig config)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new(config.BaseUrl);
            client.DefaultRequestHeaders.ApplyZaaksysteemHeaders(config, User);
            return client;
        }

        private class CopyResponseMessageResult : IStatusCodeActionResult
        {
            private readonly HttpResponseMessage _message;

            public CopyResponseMessageResult(HttpResponseMessage message)
            {
                _message = message;
            }

            public int? StatusCode => (int)_message.StatusCode;

            public async Task ExecuteResultAsync(ActionContext context)
            {
                var response = context.HttpContext.Response;
                response.StatusCode = (int)_message.StatusCode;
                response.ContentType = _message.Content.Headers.ContentType?.ToString() ?? "";
                await _message.Content.CopyToAsync(response.Body, context.HttpContext.RequestAborted);
            }
        }
    }
}
