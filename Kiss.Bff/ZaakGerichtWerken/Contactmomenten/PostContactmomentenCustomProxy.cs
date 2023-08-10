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

        public PostContactmomentenCustomProxy(IConfiguration configuration)
        {
            _destination = configuration["CONTACTMOMENTEN_BASE_URL"];
            var clientId = configuration["CONTACTMOMENTEN_API_CLIENT_ID"];
            var apiKey = configuration["CONTACTMOMENTEN_API_KEY"];

            _tokenProvider = new ZgwTokenProvider(apiKey, clientId);
        }

        [HttpPost]
        public IActionResult Post([FromBody] JsonObject parsedModel)
        {
            var email = User?.GetEmail();
            var userRepresentation = User?.Identity?.Name;
            if (parsedModel != null)
            {
                //claims zijn niet standaard. configuratie mogelijkheid vereist voor juiste vulling 
                parsedModel["medewerkerIdentificatie"] = User?.GetMedewerkerIdentificatie();
            }

            var accessToken = _tokenProvider.GenerateToken(email, userRepresentation);

            var url = _destination.TrimEnd('/') + "/contactmomenten/api/v1/contactmomenten";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return new ProxyResult(request);
        }


    }
}
