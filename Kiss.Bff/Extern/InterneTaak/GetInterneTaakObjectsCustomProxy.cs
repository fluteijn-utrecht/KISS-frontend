using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Kiss.Bff.Extern.InterneTaak
{
    [ApiController]
    public class GetInterneTaakObjectsCustomProxy(RegistryConfig registryConfig) : ControllerBase
    {
        [HttpGet("api/internetaak/api/{version}/objects")]
        public IActionResult Get([FromRoute] string version, [FromHeader(Name = "systemIdentifier")] string? systemIdentifier)
        {
            var registry = registryConfig.GetRegistrySystem(systemIdentifier)?.InterneTaakRegistry;

            if (registry == null) return BadRequest("Geen Interne Taakregister gevonden voor deze systemIdentifier");

            return new ProxyResult(() =>
            {
                var message = new HttpRequestMessage(new HttpMethod(Request.Method), GetUri(registry, $"api/{version}/objects"))
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
            url = QueryHelpers.AddQueryString(url, "type", config.ObjectTypeUrl);
            return new Uri(url);
        }
    }
}
