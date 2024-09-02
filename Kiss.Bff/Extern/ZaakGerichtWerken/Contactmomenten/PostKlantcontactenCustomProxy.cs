using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json;
using Kiss.Bff.Afdelingen;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api")]
    [ApiController]
    public class PostKlantContactenCustomProxy : ControllerBase
    {
        private readonly string _klantcontactenDestination;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;
        private readonly IAuthenticationHeaderProvider _authProvider;
        private readonly HttpClient _httpClient;

        public PostKlantContactenCustomProxy(IConfiguration configuration, GetMedewerkerIdentificatie getMedewerkerIdentificatie, IAuthenticationHeaderProvider authProvider, HttpClient httpClient)
        {
            _klantcontactenDestination = configuration["KLANTCONTACTEN_BASE_URL"];
            _authProvider = authProvider;
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
            _httpClient = httpClient;
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

        public async Task<string> PostKlantContact(JsonObject parsedModel)
        {
            var url = _klantcontactenDestination.TrimEnd('/') + "/klantinteracties/api/v1/klantcontacten";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            _authProvider.ApplyAuthorizationHeader(request.Headers, User);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(content);

            return jsonResponse.RootElement.GetProperty("uuid").GetString();
        }

        public async Task<IActionResult> PostActoren()
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

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        public async Task<string?> CheckIfActorExists(string email)
        {
            var url = $"{_klantcontactenDestination.TrimEnd('/')}/klantinteracties/api/v1/actoren";
            var queryParams = new Dictionary<string, string>
            {
                { "actoridentificatorObjectId", email }
            };

            var queryString = string.Join("&", queryParams.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{kv.Key}={kv.Value}"));
            var requestUrl = $"{url}?{queryString}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            _authProvider.ApplyAuthorizationHeader(request.Headers, User);

            var response = await _httpClient.SendAsync(request);

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

        public IActionResult LinkActorWithKlantContact(string actorUuid, string klantcontactUuid)
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

    // Interface voor unittests en misschien zoiets lokaal implementeren? 
    public interface IAuthenticationHeaderProvider
    {
        void ApplyAuthorizationHeader(HttpRequestHeaders headers, ClaimsPrincipal user);
    }

    public class AuthenticationHeaderProviderWrapper : IAuthenticationHeaderProvider
    {
        private readonly AuthenticationHeaderProvider _authProvider;

        public AuthenticationHeaderProviderWrapper(AuthenticationHeaderProvider authProvider)
        {
            _authProvider = authProvider;
        }

        public void ApplyAuthorizationHeader(HttpRequestHeaders headers, ClaimsPrincipal user)
        {
            _authProvider.ApplyAuthorizationHeader(headers, user);
        }
    }


