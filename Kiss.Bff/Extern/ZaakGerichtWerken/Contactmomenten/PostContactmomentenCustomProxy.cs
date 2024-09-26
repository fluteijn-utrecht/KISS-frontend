using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api/postcontactmomenten")]
    [ApiController]
    public class PostContactmomentenCustomProxy : ControllerBase
    {
        private readonly ZgwTokenProvider _tokenProvider;
        private readonly string _destination;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;

        public PostContactmomentenCustomProxy(IConfiguration configuration, GetMedewerkerIdentificatie getMedewerkerIdentificatie)
        {
            _destination = configuration["CONTACTMOMENTEN_BASE_URL"]; //open klant
            var clientId = configuration["CONTACTMOMENTEN_API_CLIENT_ID"];
            var apiKey = configuration["CONTACTMOMENTEN_API_KEY"];

            _tokenProvider = new ZgwTokenProvider(apiKey, clientId);
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
        }

        [HttpPost]
        public IActionResult Post([FromBody] JsonObject parsedModel)
        {
            var email = User?.GetEmail();
            var userRepresentation = User?.Identity?.Name;
            if (parsedModel != null)
            {
                parsedModel["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();
            }

            var accessToken = _tokenProvider.GenerateToken(User);

            var url = _destination.TrimEnd('/') + "/contactmomenten/api/v1/contactmomenten";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(parsedModel)
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                return request;
            });
        }
    }
}
