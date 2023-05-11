using SdgElasticPoller;


var sdgBaseUrl = GetEnvironmentVariable("SDG_BASE_URL") ?? "https://sdgtest.icatt-services.nl";
var elasticBaseUrl = GetEnvironmentVariable("ELASTIC_BASE_URL") ?? "https://sdgtest.icatt-services.nl";
var elasticIndex = GetEnvironmentVariable("ELASTIC_SDG_INDEX") ?? "my_index";

if (!Uri.TryCreate(sdgBaseUrl, UriKind.Absolute, out var sdgBaseUri))
{
    Console.Write("sdg base url is niet valide: ");
    Console.WriteLine(sdgBaseUrl);
    return;
}

if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
{
    Console.Write("elastic base url is niet valide: ");
    Console.WriteLine(elasticBaseUrl);
    return;
}

using var consoleStream = Console.OpenStandardOutput();
using var sdgClient = new SdgProductClient(sdgBaseUri);
using var elasticClient = new ElasticBulkIndexClient(elasticBaseUri);
using var cancelSource = new CancellationTokenSource();

AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.Dispose();

//await elasticClient.BulkIndexAsync(stream => sdgClient.Get("/api/v1/producten", (el) => consoleStream.WriteBulkWriteSdgIndexRequest(el, elasticIndex), cancelSource.Token), cancelSource.Token);

await sdgClient.Get("/api/v1/producten", (el) => consoleStream.WriteBulkWriteSdgIndexRequest(el, elasticIndex), cancelSource.Token);

static string? GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable("name", EnvironmentVariableTarget.Process);
