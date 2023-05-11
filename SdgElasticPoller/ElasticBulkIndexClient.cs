namespace SdgElasticPoller
{
    public sealed class ElasticBulkIndexClient: IDisposable
    {
        private readonly HttpClient _httpClient;

        public ElasticBulkIndexClient(Uri baseUri)
        {
            _httpClient = new HttpClient { BaseAddress = baseUri };
        }

        public async Task BulkIndexAsync(Func<Stream, Task> writer, CancellationToken token)
        {
            using var pushContent = new PushStreamContent(onStreamAvailable: (stream, content, context) => writer(stream));
            using var response = await _httpClient.PostAsync("_bulk", pushContent, token);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
