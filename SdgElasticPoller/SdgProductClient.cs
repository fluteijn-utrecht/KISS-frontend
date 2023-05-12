using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SdgElasticPoller
{
    public sealed class SdgProductClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public SdgProductClient(Uri baseUri)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = baseUri,
            };
        }

        public IAsyncEnumerable<JsonElement> Get(CancellationToken token) => Get("/api/v1/producten", token);

        private async IAsyncEnumerable<JsonElement> Get(string url, [EnumeratorCancellation] CancellationToken token)
        {
            await using var stream = await _httpClient.GetStreamAsync(url, token);
            using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

            var next = jsonDoc.RootElement.TryGetProperty("next", out var nextProp) && nextProp.ValueKind == JsonValueKind.String
                ? nextProp.GetString()
                : null;

            if (jsonDoc.RootElement.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in resultsProp.EnumerateArray())
                {
                    yield return item;
                }
            }

            if (!string.IsNullOrWhiteSpace(next))
            {
                await foreach (var el in Get(next, token))
                {
                    yield return el;
                }
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
