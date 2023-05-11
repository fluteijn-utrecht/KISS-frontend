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

        public async Task Get(string url, Action<JsonElement> write, CancellationToken token)
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
                    write(item);
                }
            }

            if (!string.IsNullOrWhiteSpace(next))
            {
                await Get(next, write, token);
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
