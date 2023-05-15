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

        public IAsyncEnumerable<KissEnvelope> Get(CancellationToken token) => Get("/api/v1/producten", token);

        private async IAsyncEnumerable<KissEnvelope> Get(string url, [EnumeratorCancellation] CancellationToken token)
        {
            string? next;

            await using (var stream = await _httpClient.GetStreamAsync(url, token))
            {
                using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

                next = jsonDoc.RootElement.TryGetProperty("next", out var nextProp) && nextProp.ValueKind == JsonValueKind.String
                    ? nextProp.GetString()
                    : null;

                if (jsonDoc.RootElement.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var sdgProduct in resultsProp.EnumerateArray())
                    {
                        if (!sdgProduct.TryGetProperty("uuid", out var id))
                        {
                            continue;
                        }

                        JsonElement title = default;
                        JsonElement objectMeta = default;

                        if (sdgProduct.TryGetProperty("vertalingen", out var vertalingenProp) && vertalingenProp.ValueKind == JsonValueKind.Array)
                        {
                            var vertaling = vertalingenProp[0];
                            vertaling.TryGetProperty("titel", out title);
                            vertaling.TryGetProperty("tekst", out objectMeta);
                        }

                        yield return new KissEnvelope(sdgProduct, title, objectMeta, $"kennisartikel_{id.GetString()}");
                    }
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
