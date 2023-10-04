using System.Text.Json.Nodes;
using Kiss.Bff.Groepen;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.KlantTaak
{
    [ApiController]
    public class CreateKlantTaakCustomProxy : ControllerBase
    {
        private readonly KlantTaakProxyConfig _config;

        public CreateKlantTaakCustomProxy(KlantTaakProxyConfig config)
        {
            _config = config;
        }

        [HttpPost("api/klanttaken/api/{version}/objects")]
        public IActionResult Post([FromRoute] string version, [FromBody] JsonObject node)
        {
            node["type"] = _config.ObjectTypeUrl;

            var url = $"{_config.Destination.AsSpan().TrimEnd('/')}/api/{version}/objects";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(node) };
                _config.ApplyHeaders(request.Headers);
                return request;
            });
        }
    }
}
