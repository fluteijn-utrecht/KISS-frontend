using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SdgElasticPoller
{
    public sealed class ElasticBulkIndexClient : IDisposable
    {
        const int MaxRequestSizeInMegabytes = 10;
        const int MaxPositionInBytes = MaxRequestSizeInMegabytes * 1000 * 1000;

        private readonly HttpClient _httpClient;

        public ElasticBulkIndexClient(Uri baseUri, string username, string password)
        {
            _httpClient = new HttpClient { BaseAddress = baseUri };
            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", base64);
        }

        public async Task BulkIndexAsync(IAsyncEnumerable<JsonElement> documents, string elasticIndex, CancellationToken token)
        {
            await EnsureIndex(elasticIndex, token);

            await using var enumerator = documents.GetAsyncEnumerator(token);
            while (true)
            {
                var hasData = false;

                await using var fs = new FileStream(
                    Path.GetTempFileName(),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    4096,
                    FileOptions.RandomAccess | FileOptions.DeleteOnClose);
                const byte NewLine = (byte)'\n';

                var writeNewLine = () => fs.WriteByte(NewLine);

                using var writer = new Utf8JsonWriter(fs);

                while (await enumerator.MoveNextAsync())
                {
                    hasData = true;
                    await writer.WriteBulkSdgIndexRequestAsync(writeNewLine, enumerator.Current, elasticIndex, token);
                    if (fs.Position >= MaxPositionInBytes)
                    {
                        break;
                    }
                }

                if (!hasData) break;

                fs.Seek(0, SeekOrigin.Begin);
                fs.Position = 0;
                var content = new StreamContent(fs);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-ndjson");
                using var response = await _httpClient.PostAsync("_bulk", content, token);
                await using var responseStream = await response.Content.ReadAsStreamAsync(token);
                await using var stdOutStream = response.IsSuccessStatusCode
                    ? Console.OpenStandardOutput()
                    : Console.OpenStandardError();
                await responseStream.CopyToAsync(stdOutStream, token);
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task EnsureIndex(string elasticIndex, CancellationToken token)
        {
            using var existsRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Head,
                RequestUri = new Uri(_httpClient.BaseAddress!, elasticIndex)
            };
            using var existsResult = await _httpClient.SendAsync(existsRequest, token);
            if (!existsResult.IsSuccessStatusCode)
            {

                using var createResult = await _httpClient.PutAsync(elasticIndex, null, token);
                createResult.EnsureSuccessStatusCode();
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
