using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.InterneTaak
{
    [ApiController]
    public class PostInterneTaakCustomProxy : ControllerBase
    {
        private readonly InterneTaakProxyConfig _config;

        public PostInterneTaakCustomProxy(InterneTaakProxyConfig config)
        {
            _config = config;
        }

        [HttpPost("api/internetaak/api/{version}/objects")]
        public IActionResult Post([FromRoute] string version, [FromBody] JsonObject node)
        {
            node["type"] = _config.ObjectTypeUrl;

            if (node.TryGetPropertyValue("record", out var record)
                && record is JsonObject recordObj
                && recordObj.TryGetPropertyValue("data", out var data)
                && data is JsonObject dataObj)
            {
                dataObj["medewerkerIdentificatie"] = User.GetMedewerkerIdentificatie();
            }

            var url = $"{_config.Destination.AsSpan().TrimEnd('/')}/api/{version}/objects";
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(node) };
            _config.ApplyHeaders(request.Headers);

            return new ProxyResult(request);
        }
    }
}
