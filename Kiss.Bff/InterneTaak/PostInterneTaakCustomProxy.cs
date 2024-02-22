using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.InterneTaak
{
    [ApiController]
    public class PostInterneTaakCustomProxy : ControllerBase
    {
        private readonly InterneTaakProxyConfig _config;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;

        public PostInterneTaakCustomProxy(InterneTaakProxyConfig config, GetMedewerkerIdentificatie getMedewerkerIdentificatie)
        {
            _config = config;
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
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
                dataObj["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();
            }

            var url = $"{_config.Destination.AsSpan().TrimEnd('/')}/api/{version}/objects";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(node) };
                _config.ApplyHeaders(request.Headers, User);
                return request;
            });
        }
    }
}
