using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Klantinteracties
{
    [ApiController]
    [Route("/api/klantinteracties/{**path}")]
    public class KlantinteractiesProxy(RegistryConfig registryConfig) : ControllerBase
    {
        [HttpPost]
        [HttpGet]
        public IActionResult Handle([FromRoute] string path, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = registryConfig.GetRegistrySystem(systemIdentifier)?.KlantinteractieRegistry;

            if (registry == null) return BadRequest("Geen Klantinteractieregister gevonden voor deze systemIdentifier");

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

        private Uri GetUri(KlantinteractieRegistry config, string path) => new Uri($"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request.QueryString}");
    }
}
