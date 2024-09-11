using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json;
using Kiss.Bff.Afdelingen;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kiss.Bff.Extern.Klantinteracties
{
    [Route("api")]
    [ApiController]
    public class PostContactVerzoek : ControllerBase
    {
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;
        private readonly HttpClient _httpClient;
        private readonly KlantinteractiesProxyConfig _klantinteractiesProxyConfig;

        public PostContactVerzoek(GetMedewerkerIdentificatie getMedewerkerIdentificatie, HttpClient httpClient, KlantinteractiesProxyConfig klantinteractiesProxyConfig)
        {
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
            _httpClient = httpClient;
            _klantinteractiesProxyConfig = klantinteractiesProxyConfig;
        }

        [HttpPost("postinternetaak")]
        public IActionResult PostInterneTaak([FromBody] JsonObject parsedModel)
        {
            if (parsedModel != null)
            {
                parsedModel["medewerkerIdentificatie"] = _getMedewerkerIdentificatie(); 
            }

            var url = _klantinteractiesProxyConfig.Destination.TrimEnd('/') + "/api/v1/internetaken";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parsedModel) };
                _klantinteractiesProxyConfig.ApplyHeaders(request.Headers, ControllerContext.HttpContext.User);
                return request;
            });
        }
    }
}



