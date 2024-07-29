using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemGetEndpoints : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ZaaksysteemGetEndpoints> _logger;

        public ZaaksysteemGetEndpoints(
            IEnumerable<ZaaksysteemConfig> zakenProxyConfigs,
            IHttpClientFactory httpClientFactory,
            ILogger<ZaaksysteemGetEndpoints> logger)
        {
            _configs = zakenProxyConfigs;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("api/zaken/{**path}")]
        [HttpGet("api/documenten/{**path}")]
        public Task<IActionResult> Get(
            string path,
            [FromHeader(Name = "ZaaksysteemId")] string? zaaksysteemId,
            [FromQuery(Name = "ordering")] IEnumerable<string> ordering,
            CancellationToken token) =>
            IsRequestForSingleEntity(path)
                ? GetSingleEntity(path, zaaksysteemId, token)
                : GetListOfEntities(path, zaaksysteemId, ordering, token);

        private static bool IsRequestForSingleEntity(ReadOnlySpan<char> path)
        {
            const int GuidLength = 36;
            path = path.TrimEnd('/');
            if (path.Length < GuidLength) return false;
            var start = path.Length - GuidLength;
            return Guid.TryParse(path.Slice(start), out _);
        }

        private async Task<IActionResult> GetSingleEntity(string path, string? zaaksysteemId, CancellationToken token)
        {
            var messages = await SendRequestToZaaksystemen(path, zaaksysteemId, token);

            if (messages.Length == 0)
            {
                return GeenZaaksysteemGeconfigureerdError();
            }

            var (baseAddress, successMessage) = messages.FirstOrDefault(x => x.Message?.IsSuccessStatusCode ?? false);

            if (successMessage == null)
            {
                var (_, errorMessage) = messages.FirstOrDefault(x => x.Message != null);
                if (errorMessage == null)
                {
                    return GeenZaaksysteemBeschikbaarError();
                }

                return new CopyResponseMessageResult(errorMessage);
            }

            var isJson = successMessage.Content.Headers.ContentType?.MediaType?.Contains("json") ?? false;

            if (!isJson)
            {
                return new CopyResponseMessageResult(successMessage);
            }

            var json = await successMessage.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: token);
            json!.AddZaaksysteemId(baseAddress?.ToString());
            return StatusCode((int)successMessage.StatusCode, json);
        }

        private async Task<IActionResult> GetListOfEntities(string path, string? zaaksysteemId, IEnumerable<string> ordering, CancellationToken token)
        {
            var messages = await SendRequestToZaaksystemen(path, zaaksysteemId, token);

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
                    return (IsPaginated: true, Array: a.Select(n => n?.AddZaaksysteemId(x.BaseAddress!.ToString())));

                if (node is JsonArray arr)
                    return (IsPaginated: false, Array: arr.Select(n => n?.AddZaaksysteemId(x.BaseAddress!.ToString())));

                // we krijgen EN geen lijst EN geen gepagineerd resultaat terug
                // negeren?
                return default;
            });

            var nodes = await Task.WhenAll(tasks);

            var allArePaginated = nodes.All(x => x.IsPaginated);
            var noneArePaginated = nodes.All(x => !x.IsPaginated);

            // als we een mengeling krijgen van gepagineerde en ongepagineerde resultaten, is er iets raars aan de hand
            if (!allArePaginated
                && !noneArePaginated)
            {
                return Problem(
                    title: "Verschillen in paginering tussen zaaksystemen",
                    statusCode: 502
                );
            }

            // als er zaaksystemen zijn die een foutmelding hebben opgeleverd, geven we dit terug als header
            // zodat we dat evt aan de voorkant kunnen tonen
            var hasErrors = messages.Any(x => x.Message == null || !x.Message.IsSuccessStatusCode);

            if (hasErrors)
            {
                Response?.Headers?.Add("Has-Errors", "True");
            }

            var merged = nodes
                .SelectMany(x => x.Array)
                .OfType<JsonObject>()
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

            if (noneArePaginated)
            {
                return Ok(merged);
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
        private Task<(Uri? BaseAddress, HttpResponseMessage? Message)[]> SendRequestToZaaksystemen(string path, string? zaaksysteemId, CancellationToken token)
        {
            var tasks = _configs.FilterByZaakSysteemId(zaaksysteemId)
                .Select(CreateClient)
                .Select(async (client) =>
                {
                    try
                    {
                        var message = await client.GetAsync($"{path}{Request?.QueryString}", HttpCompletionOption.ResponseHeadersRead, token);
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

        private HttpClient CreateClient(ZaaksysteemConfig config)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new(config.BaseUrl);
            client.DefaultRequestHeaders.ApplyZaaksysteemHeaders(config, User);
            return client;
        }

        private IActionResult GeenZaaksysteemBeschikbaarError() => Problem(
            title: "Configuratieprobleem",
            detail: "Er is geen enkel zaaksysteem bereikbaar",
            statusCode: 502
        );

        private IActionResult GeenZaaksysteemGeconfigureerdError() => Problem(
            title: "Configuratieprobleem",
            detail: "Er is geen zaaksysteem geconfigureerd",
            statusCode: 400
        );

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
