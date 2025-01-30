
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.InterneTaak
{
    [ApiController]
    [Route("/api/internetaak/{**path}")]
    public class InterneTaakObjectsProxy(RegistryConfig registryConfig) : ControllerBase
    {
        [HttpPost]
        [HttpGet]
        public IActionResult Handle([FromRoute] string path, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = registryConfig.GetRegistrySystem(systemIdentifier)?.InterneTaakRegistry;

            if (registry == null) return BadRequest("Geen Interne Taakregister gevonden voor deze systemIdentifier");

            return new ProxyResult(() =>
            {
                var message = new HttpRequestMessage(new HttpMethod(Request.Method), GetUri(registry, path))
                {
                    Content = new StreamContent(Request.Body)
                };
                if (!string.IsNullOrWhiteSpace(Request.ContentType))
                {
                    message.Content.Headers.ContentType = new(Request.ContentType);
                }
                message.Content.Headers.ContentLength = Request.ContentLength;
                registry.ApplyHeaders(message.Headers, User);
                return message;
            });
        }

        private Uri GetUri(InternetaakRegistry config, string path)
        {
            var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request.QueryString}";
            return new Uri(url);
        }
    }
}
