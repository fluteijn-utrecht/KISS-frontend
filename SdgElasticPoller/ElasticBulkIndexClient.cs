using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SdgElasticPoller
{
    public sealed class ElasticBulkIndexClient : IDisposable
    {
        const int MaxRequestSizeInMegabytes = 10;
        const int MaxPositionInBytes = MaxRequestSizeInMegabytes * 1000 * 1000;
        const byte NewLine = (byte)'\n';

        private readonly HttpClient _httpClient;

        public ElasticBulkIndexClient(Uri baseUri, string username, string password)
        {
            _httpClient = new HttpClient { BaseAddress = baseUri };
            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", base64);
        }

        public async Task BulkIndexAsync(IAsyncEnumerable<KissEnvelope> documents, string elasticIndex, string bron, CancellationToken token)
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

                using var jsonWriter = new Utf8JsonWriter(fs);

                while (await enumerator.MoveNextAsync())
                {
                    hasData = true;
                    await WriteBulkRequestAsync(elasticIndex, bron, enumerator, fs, jsonWriter, token);
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
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "_bulk");
                requestMessage.Content = content;
                // https://www.stevejgordon.co.uk/using-httpcompletionoption-responseheadersread-to-improve-httpclient-performance-dotnet
                using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
                await using var responseStream = await response.Content.ReadAsStreamAsync(token);
                await using var stdOutStream = response.IsSuccessStatusCode
                    ? Console.OpenStandardOutput()
                    : Console.OpenStandardError();
                await responseStream.CopyToAsync(stdOutStream, token);
                response.EnsureSuccessStatusCode();
            }
        }

        private static async Task WriteBulkRequestAsync(string elasticIndex, string bron, IAsyncEnumerator<KissEnvelope> enumerator, FileStream fs, Utf8JsonWriter jsonWriter, CancellationToken token)
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("index");
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("_index", elasticIndex);
            jsonWriter.WriteString("_id", enumerator.Current.Id);

            jsonWriter.WriteEndObject();
            await jsonWriter.FlushAsync(token);
            jsonWriter.Reset();
            fs.WriteByte(NewLine);
            enumerator.Current.WriteTo(jsonWriter, bron);
            await jsonWriter.FlushAsync(token);
            jsonWriter.Reset();
            fs.WriteByte(NewLine);
        }

        private async Task EnsureIndex(string elasticIndex, CancellationToken token)
        {
            using var existsRequest = new HttpRequestMessage(HttpMethod.Head, elasticIndex);
            using var existsResult = await _httpClient.SendAsync(existsRequest, HttpCompletionOption.ResponseHeadersRead, token);
            if (!existsResult.IsSuccessStatusCode)
            {
                using var putRequest = new HttpRequestMessage(HttpMethod.Put, elasticIndex);
                using var createResult = await _httpClient.SendAsync(putRequest, HttpCompletionOption.ResponseHeadersRead, token);
                createResult.EnsureSuccessStatusCode();
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
