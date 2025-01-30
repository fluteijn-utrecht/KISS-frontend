using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.InterneTaak
{
    [ApiController]
    public class PostInterneTaakObjectCustomProxy : ControllerBase
    {
        private readonly RegistryConfig _config;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;

        public PostInterneTaakObjectCustomProxy(RegistryConfig config, GetMedewerkerIdentificatie getMedewerkerIdentificatie)
        {
            _config = config;
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
        }

        [HttpPost]
        [Route("api/internetaak/api/{version}/objects")]
        public IActionResult Post([FromRoute] string version, [FromBody] JsonObject node, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            var registry = _config.GetRegistrySystem(systemIdentifier)?.InterneTaakRegistry;

            if (registry == null) return BadRequest("Geen Interne Taakregister gevonden voor deze systemIdentifier");

            node["type"] = registry.ObjectTypeUrl;

            if (node.TryGetPropertyValue("record", out var record) && record is JsonObject recordObj)
            {
                recordObj["typeVersion"] = registry.ObjectTypeVersion;

                if (recordObj.TryGetPropertyValue("data", out var data) && data is JsonObject dataObj)
                {
                    dataObj["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();
                }
            }

            var url = $"{registry.BaseUrl.TrimEnd('/')}/api/{version}/objects";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(node) };
                registry.ApplyHeaders(request.Headers, ControllerContext.HttpContext.User);
                return request;
            });
        }
    }
}
