using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.InterneTaak
{
    [ApiController]
    public class PostInterneTaakCustomProxy : ControllerBase
    {
        private readonly InterneTaakSettings _settings;

        public PostInterneTaakCustomProxy(InterneTaakSettings settings)
        {
            _settings = settings;
        }

        [HttpPost("api/internetaak/api/{version}/objects")]
        public IActionResult Post([FromRoute] string version, [FromBody] JsonObject node)
        {
            node["type"] = _settings.ObjectTypeUrl;

            if (node.TryGetPropertyValue("record", out var record)
                && record is JsonObject recordObj
                && recordObj.TryGetPropertyValue("data", out var data)
                && data is JsonObject dataObj)
            {
                dataObj["medewerkerIdentificatie"] = User.GetMedewerkerIdentificatie();
            }

            var url = $"{_settings.Destination.AsSpan().TrimEnd('/')}/api/{version}/objects";
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(node) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", _settings.Token);
            request.Headers.Add("Content-Crs", "EPSG:4326");

            return new ProxyResult(request);
        }
    }
}
