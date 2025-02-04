using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Contactmomenten
{
    [ApiController]
    [Route("/api/contactmomenten/{**path}")]
    public class ContactmomentenProxy(RegistryConfig registryConfig) : ControllerBase
    {
        private readonly RegistryConfig _registryConfig = registryConfig;

        [HttpPost]
        [HttpGet]
        public IActionResult Handle([FromRoute] string path, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = string.IsNullOrWhiteSpace(systemIdentifier)
                ? _registryConfig.Systemen.FirstOrDefault(x => x.IsDefault)?.KlantinteractieRegistry
                : _registryConfig.Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier)?.KlantinteractieRegistry;

            if (registry == null)
            {
                return BadRequest();
            }

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

        private Uri GetUri(KlantinteractieRegistry config, string path) => new Uri($"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}?{Request.QueryString}");
    }
}
