using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IdentityModel;
using System.Security.Claims;
using System.Net;
using AngleSharp.Io;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Xml;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api/postcontactmomenten")]
    [ApiController]
    public class PostContactmomentenCustomProxy : ControllerBase
    {
        private readonly HttpClient _defaultClient;
        private readonly ZgwTokenProvider _tokenProvider;
        public PostContactmomentenCustomProxy(IConfiguration configuration, IHttpClientFactory factory)
        {
            var destination = configuration["CONTACTMOMENTEN_BASE_URL"];
            var clientId = configuration["CONTACTMOMENTEN_API_CLIENT_ID"];
            var apiKey = configuration["CONTACTMOMENTEN_API_KEY"];

            _defaultClient = factory.CreateClient();
            _tokenProvider = new ZgwTokenProvider(apiKey, clientId);


        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] System.Text.Json.JsonElement entity)
        {
            var userId = Request.HttpContext.User?.FindFirstValue(JwtClaimTypes.PreferredUserName);
            var userRepresentation = Request.HttpContext.User?.Identity?.Name;


            var model = entity.GetRawText();
            var parsedModel = JsonNode.Parse(model);


            if (parsedModel != null)
            {

                parsedModel["medewerkerIdentificatie"] = new JsonObject
                {
                    ["achternaam"] = userRepresentation,
                    ["identificatie"] = userId,
                    ["voorletters"] = "",
                    ["voorvoegselAchternaam"] = "",

                };

            }



            var accessToken = _tokenProvider.GenerateToken(userId, userRepresentation);

          

            var url = "https://open-klant.dev.kiss-demo.nl/contactmomenten/api/v1/contactmomenten";


            //   using var newRequest = new HttpRequestMessage(new System.Net.Http.HttpMethod(Request.Method), url);

            //  newRequest.Content = new StreamContent(Request.HttpContext.Request.Body);

            _defaultClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //  return await httpClient.PostAsync(url);

            //     return await _defaultClient.PostAsJsonAsync(url, xx );

            return await _defaultClient.PostAsJsonAsync(url, parsedModel);



        }

    }
}
