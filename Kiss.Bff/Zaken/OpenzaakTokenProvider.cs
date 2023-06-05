using Kiss.Bff.Zaken.Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kiss.Bff.Zaken
{

    public static class ZgwTokenExtensions
    {
        public static IServiceCollection AddZgwTokenProvider(this IServiceCollection services, string apiKey, string clientId)
        {
            services.AddSingleton<ZgwTokenProvider>(new ZgwTokenProvider(apiKey, clientId));
            return services;
        }
    }

    //documentatie: https://open-zaak.readthedocs.io/en/latest/client-development/authentication.html
    public class ZgwTokenProvider
    {
        private readonly string _apiKey;
        private readonly string _clientId;

        public ZgwTokenProvider(string apiKey, string clientId)
        {
            _apiKey = apiKey;
            _clientId = clientId;
        }


        public string GenerateToken(string? userId, string? userRepresentation)
        {

            var secretKey = _apiKey; // "een sleutel van minimaal 16 karakters";
            var client_id = _clientId; 
            var iss = _clientId; 
            var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); 
            var user_id = userId ?? string.Empty;
            var user_representation = userRepresentation ?? string.Empty;


            var claims = new Dictionary<string, object>
                {
                    { "client_id", client_id },
                    { "iss", iss },
                    { "iat", iat },
                    { "user_id", user_id},
                    { "user_representation", user_representation }
                };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                Subject = new ClaimsIdentity(),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);



        }
    }
}
