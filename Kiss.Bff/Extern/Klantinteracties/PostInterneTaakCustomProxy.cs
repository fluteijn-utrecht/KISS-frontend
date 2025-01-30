using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace Kiss.Bff.Extern.Klantinteracties
{
    public class PostInterneTaakCustomProxy(RegistryConfig registryConfig) : ControllerBase
    {
        [HttpPost("api/postinternetaak")]
        public IActionResult PostInterneTaak([FromBody] JsonObject parsedModel, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            var config = registryConfig.Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier)?.KlantinteractieRegistry;

            if (config == null) return BadRequest();

            var url = config.BaseUrl.TrimEnd('/') + "/api/v1/internetaken";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parsedModel) };
                config.ApplyHeaders(request.Headers, User);
                return request;
            });
        }
    }
}
