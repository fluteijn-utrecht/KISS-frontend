using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using IdentityModel;
using Kiss.Bff;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Zaken
{

    namespace Microsoft.Extensions.DependencyInjection
    {
        public static class ZakenExtensions
        {
            public static IServiceCollection AddZaken(this IServiceCollection services, string baseUrl )
            {
                services.AddSingleton<IKissProxyRoute>((x) => {
                    var zgwToken = x.GetRequiredService<ZgwTokenProvider>();                    
                    return new ZakenProxyConfig(baseUrl, zgwToken); 
                });

                return services;
            }
        }

        public sealed class ZakenProxyConfig : IKissProxyRoute
        { 
            private readonly ZgwTokenProvider _zgwTokenProvider;

            public ZakenProxyConfig(string destination, ZgwTokenProvider zgwTokenProvider)
            {
                Destination = destination;           
                _zgwTokenProvider = zgwTokenProvider;
            }

            public string Route => "zaken";
            public string Destination { get; }

            public IServiceProvider ServiceProvider => throw new NotImplementedException();

            //documentatie: https://open-zaak.readthedocs.io/en/latest/client-development/authentication.html
            public ValueTask ApplyRequestTransform(RequestTransformContext context)
            {
                var userId = context.HttpContext.User?.FindFirstValue(JwtClaimTypes.PreferredUserName);
                var userRepresentation = context.HttpContext.User?.Identity?.Name;

                var token = _zgwTokenProvider.GenerateToken(userId, userRepresentation);

                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                context.ProxyRequest.Headers.Add("Accept-Crs", "EPSG:4326"); //voorlopig eignelijk niet nodig. wordt pas relevant wanneer we geografische coordinaten gaan opvragen

                return new();
            }
         
        }
    }

}
