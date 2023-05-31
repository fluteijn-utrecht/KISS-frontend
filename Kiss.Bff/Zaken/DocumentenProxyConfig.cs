using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Kiss.Bff;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Zaken
{

    namespace Microsoft.Extensions.DependencyInjection
    {
        public static class DocumentenExtensions
        {
            public static IServiceCollection AddDocumenten(this IServiceCollection services, string baseUrl, string apiKey)
            {
                services.AddSingleton<IKissProxyRoute>(new DocumentenProxyConfig(baseUrl, apiKey));
                return services;
            }
        }

        public sealed class DocumentenProxyConfig : IKissProxyRoute
        {
            private readonly string _apiKey;

            public DocumentenProxyConfig(string destination, string apiKey)
            {
                Destination = destination;
                _apiKey = apiKey;
            }

            public string Route => "documenten";
            public string Destination { get; }

            public ValueTask ApplyRequestTransform(RequestTransformContext context)
            {
                //hier het tijdelijk geldige token uit https://open-zaak.dev.kiss-demo.nl/admin/authorizations/applicatie/1/change/ invullen
                //var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJiZmZfY2xpZW50IiwiaWF0IjoxNjg1MDkzNzY0LCJjbGllbnRfaWQiOiJiZmZfY2xpZW50IiwidXNlcl9pZCI6IiIsInVzZXJfcmVwcmVzZW50YXRpb24iOiIifQ.ITz3VrY8dxzwayxZ3SVaMCbfrMtDJql1ZtEn-dv8s14";

                var token = GenerateToken();

                //token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0IiwiaWF0IjoxNjg1MTA4Njk3LCJjbGllbnRfaWQiOiJ0ZXN0IiwidXNlcl9pZCI6IiIsInVzZXJfcmVwcmVzZW50YXRpb24iOiIifQ.JFkS-YGp1tElRF-W9412B__WPpTuDsRafvVi1n20Ji0";

                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                context.ProxyRequest.Headers.Add("Accept-Crs", "EPSG:4326"); //todo uitzoeken. moet dit?? en moet dit configurabel zijn?
                return new();
            }


            //Todo: duplicaat van de zakenproxyconfig > samenvoegen
            private  string GenerateToken()
            {
                try
                {

                    var secretKey = "eensleutelvanminimaal16karakters";


                    var client_id = "test";
                    var iss = "test";
                    var iat = 1602857301;
                    var user_id = "123";
                    var user_representation = "icatt tester";


                    var claims = new Dictionary<string, object>
                {
                    { "client_id", client_id },
                    { "iss", iss },
                    { "iat", iat },
                    { "user_id", user_id},
                    { "user_representation", user_representation }
                };

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(secretKey);
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
                catch (Exception ex)
                {

                    throw;
                }

                
            }
        }
    }

}
