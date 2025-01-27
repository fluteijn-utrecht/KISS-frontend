﻿using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.Json;
using Kiss.Bff.Afdelingen;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kiss.Bff.Extern.Klantinteracties
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

            var actorUuid = await GetActorId(email);

            if (actorUuid == null)
            {
                actorUuid = await PostActoren();

                if (actorUuid == null)
                {
                    return BadRequest(new { errorMessage = "Failed to create actor." });
                }
            }

            var klantcontact = await PostKlantContact(parsedModel);

            var linkSuccessful = await LinkActorWithKlantContact(actorUuid, klantcontact["uuid"].ToString());

            if (!linkSuccessful)
            {
                return BadRequest(new { errorMessage = "Failed to link actor with klantcontact." });
            }

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


        public async Task<string> PostActoren()
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
            var jsonResponse = JsonNode.Parse(content);

            var actorUuid = jsonResponse?["uuid"]?.ToString();

            return actorUuid ?? throw new Exception("Failed to retrieve actor UUID.");
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

        public async Task<bool> LinkActorWithKlantContact(string actorUuid, string klantcontactUuid)
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

            return response.IsSuccessStatusCode;
        }
    }
}



