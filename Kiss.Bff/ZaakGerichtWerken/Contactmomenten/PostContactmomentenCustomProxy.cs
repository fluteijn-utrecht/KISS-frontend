using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IdentityModel;
using System.Security.Claims;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [Route("api/postcontactmoment")]
    [ApiController]
    public class PostContactmomentenCustomProxy : ControllerBase
    {

        private readonly ZgwTokenProvider _tokenProvider;
        public PostContactmomentenCustomProxy( IConfiguration configuration){
            var destination = configuration["CONTACTMOMENTEN_BASE_URL"];
            var clientId = configuration["CONTACTMOMENTEN_API_CLIENT_ID"];
            var apiKey = configuration["CONTACTMOMENTEN_API_KEY"];
 
        
            _tokenProvider = new ZgwTokenProvider(apiKey, clientId);


        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {

            var userId = Request.HttpContext.User?.FindFirstValue(JwtClaimTypes.PreferredUserName);
            var userRepresentation = Request.HttpContext.User?.Identity?.Name;

            var accessToken = _tokenProvider.GenerateToken(userId, userRepresentation);

 //read the body...
 //add medewerker info 
 //pass to openklant

            using (var httpClient = new HttpClient())
            {
                var url = "https://open-klant.dev.kiss-demo.nl/contactmoment/api/v1/contactmomenten";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return await httpClient.GetAsync(url);
             
            }

            


        }

    }
}
