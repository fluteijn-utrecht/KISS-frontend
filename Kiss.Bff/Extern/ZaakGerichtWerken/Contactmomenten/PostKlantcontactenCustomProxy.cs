using System.IO;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json;
using Kiss.Bff.Afdelingen;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api")]
    [ApiController]
    public class PostKlantContactenCustomProxy : ControllerBase
    {
        private readonly string _klantcontactenDestination;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;
        private readonly AuthenticationHeaderProvider _authProvider;

        public PostKlantContactenCustomProxy(IConfiguration configuration, GetMedewerkerIdentificatie getMedewerkerIdentificatie)
        {
            _klantcontactenDestination = configuration["KLANTCONTACTEN_BASE_URL"]; //open klant 2.0
            var token = configuration["KLANTCONTACTEN_API_KEY"];
            _authProvider = new AuthenticationHeaderProvider(token, clientId: string.Empty, clientSecret: string.Empty);
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
        }

        [HttpPost("postklantcontacten")]
        public async Task<IActionResult> PostKlantContacten([FromBody] JsonObject parsedModel)
        {
            var email = User?.GetEmail();
            var userRepresentation = User?.Identity?.Name;

            if (parsedModel != null)
            {
                parsedModel["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();
            }

            string? actorUuid = await CheckIfActorExists(email);

            if (actorUuid == null)
            {
                var postActorResult = await PostActoren();
                if (!(postActorResult is OkObjectResult okActorResult))
                {
                    return postActorResult;
                }

                var actorResponseJson = okActorResult.Value?.ToString();
                var actorResponse = JsonDocument.Parse(actorResponseJson);
                actorUuid = actorResponse.RootElement.GetProperty("uuid").GetString();
            }

            var klantcontactUuid = await PostKlantContact(parsedModel);

            return LinkActorWithKlantContact(actorUuid, klantcontactUuid);
        }

        private async Task<string> PostKlantContact(JsonObject parsedModel)
        {
            var url = _klantcontactenDestination.TrimEnd('/') + "/klantinteracties/api/v1/klantcontacten";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            _authProvider.ApplyAuthorizationHeader(request.Headers, User);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(content);

            return jsonResponse.RootElement.GetProperty("uuid").GetString();
        }

        private async Task<IActionResult> PostActoren()
        {
            var email = User?.GetEmail();
            var firstName = User?.GetFirstName();
            var lastName = User?.GetLastName();
            var fullName = $"{firstName} {lastName}".Trim();

            var parsedModel = new JsonObject
            {
                ["naam"] = fullName,
                ["soortActor"] = "medewerker",
                ["indicatieActief"] = true,
                ["actoridentificator"] = new JsonObject
                {
                    ["objectId"] = email,
                    ["codeObjecttype"] = "mdw",
                    ["codeRegister"] = "msei",
                    ["codeSoortObjectId"] = "email"
                }
            };

            parsedModel["medewerkerIdentificatie"] = _getMedewerkerIdentificatie();

            var url = _klantcontactenDestination.TrimEnd('/') + "/klantinteracties/api/v1/actoren";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            _authProvider.ApplyAuthorizationHeader(request.Headers, User);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        private async Task<string?> CheckIfActorExists(string email)
        {
            var url = $"{_klantcontactenDestination.TrimEnd('/')}/klantinteracties/api/v1/actoren";
            var queryParams = new Dictionary<string, string>
    {
        { "actoridentificatorObjectId", email }
    };

            var queryString = string.Join("&", queryParams.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{kv.Key}={kv.Value}"));
            var requestUrl = $"{url}?{queryString}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                _authProvider.ApplyAuthorizationHeader(request.Headers, User);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(content);

                    var results = jsonResponse?["results"]?.AsArray();
                    if (results != null && results.Count > 0)
                    {
                        return results[0]?["uuid"]?.ToString();
                    }
                }
                return null; 
            }
        }


        private IActionResult LinkActorWithKlantContact(string actorUuid, string klantcontactUuid)
        {
            var url = _klantcontactenDestination.TrimEnd('/') + "/klantinteracties/api/v1/actorklantcontacten";

            var payload = new JsonObject
            {
                ["actor"] = new JsonObject { ["uuid"] = actorUuid },
                ["klantcontact"] = new JsonObject { ["uuid"] = klantcontactUuid }
            };

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };

                _authProvider.ApplyAuthorizationHeader(request.Headers, User);

                return request;
            });
        }
    }
}
