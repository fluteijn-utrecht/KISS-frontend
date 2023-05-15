using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SdgElasticPoller
{
    public sealed class ElasticEnterpriseSearchClient : IDisposable
    {
        const int MaxDocuments = 100;

        private readonly HttpClient _httpClient;

        public ElasticEnterpriseSearchClient(Uri baseUri, string apiKey)
        {
            _httpClient = new HttpClient { BaseAddress = baseUri };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task IndexDocumentsAsync(IAsyncEnumerable<KissEnvelope> documents, string engine, string bron, CancellationToken token)
        {
            await using var standardOutput = Console.OpenStandardOutput();
            await using var standardError = Console.OpenStandardError();
            var url = $"/api/as/v1/engines/{engine}/documents";
            await using var enumerator = documents.GetAsyncEnumerator(token);
            var hasData = await enumerator.MoveNextAsync();

            while (hasData)
            {
                using var content = new PushStreamContent(async (stream) =>
                {
                    using var jsonWriter = new Utf8JsonWriter(stream);
                    var count = 0;
                    jsonWriter.WriteStartArray();

                    while (true)
                    {
                        enumerator.Current.WriteTo(jsonWriter, bron);
                        await jsonWriter.FlushAsync(token);
                        hasData = await enumerator.MoveNextAsync();
                        count++;
                        if (!hasData || count >= MaxDocuments)
                        {
                            jsonWriter.WriteEndArray();
                            break;
                        }
                    }
                });

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = content;
                // https://www.stevejgordon.co.uk/using-httpcompletionoption-responseheadersread-to-improve-httpclient-performance-dotnet
                using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
                await using var responseStream = await response.Content.ReadAsStreamAsync(token);
                var outputStream = response.IsSuccessStatusCode
                    ? standardOutput
                    : standardError;
                await responseStream.CopyToAsync(outputStream, token);
                response.EnsureSuccessStatusCode();
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
