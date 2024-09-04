using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json;
using Kiss.Bff.Afdelingen;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Kiss.Bff.Extern.Klantinteracties;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api")]
    [ApiController]
    public class PostKlantContactenCustomProxy : ControllerBase
    {
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;
        private readonly HttpClient _httpClient;
        private readonly KlantinteractiesProxyConfig _klantinteractiesProxyConfig;

        public PostKlantContactenCustomProxy(GetMedewerkerIdentificatie getMedewerkerIdentificatie, HttpClient httpClient, KlantinteractiesProxyConfig klantinteractiesProxyConfig)
        {
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
            _httpClient = httpClient;
            _klantinteractiesProxyConfig = klantinteractiesProxyConfig;
        }

        [HttpPost("postklantcontacten")]
        public async Task<IActionResult> PostKlantContacten([FromBody] JsonObject parsedModel)
        {
            var email = User?.GetEmail();
            var userRepresentation = User?.Identity?.Name;

            string? actorUuid = await GetActorId(email);

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

            var klantcontact = await PostKlantContact(parsedModel);
            await LinkActorWithKlantContact(actorUuid, klantcontact["uuid"].ToString());

            return Ok(klantcontact);
        }

        public async Task<JsonObject> PostKlantContact(JsonObject parsedModel)
        {
            var url = _klantinteractiesProxyConfig.Destination.TrimEnd('/') + "/api/v1/klantcontacten";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            _klantinteractiesProxyConfig.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);  
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonObject>();

            return jsonResponse;
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

            var url = _klantinteractiesProxyConfig.Destination.TrimEnd('/') + "/api/v1/actoren";
            
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parsedModel)
            };

            _klantinteractiesProxyConfig.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        public async Task<string?> GetActorId(string email)
        {
            var url = $"{_klantinteractiesProxyConfig.Destination.TrimEnd('/')}/api/v1/actoren?actoridentificatorObjectId={email}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            _klantinteractiesProxyConfig.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);

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

        public async Task<IActionResult> LinkActorWithKlantContact(string actorUuid, string klantcontactUuid)
        {
            var url = _klantinteractiesProxyConfig.Destination.TrimEnd('/') + "/api/v1/actorklantcontacten";

            var payload = new JsonObject
            {
                ["actor"] = new JsonObject { ["uuid"] = actorUuid },
                ["klantcontact"] = new JsonObject { ["uuid"] = klantcontactUuid }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };

            _klantinteractiesProxyConfig.ApplyHeaders(request.Headers, User);

            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return Ok("Actor successfully linked with KlantContact.");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, errorMessage);
        }

    }
}



