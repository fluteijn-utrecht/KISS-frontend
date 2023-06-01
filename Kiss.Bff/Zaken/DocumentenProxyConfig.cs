using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using IdentityModel;
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
            public static IServiceCollection AddDocumenten(this IServiceCollection services, string baseUrl)
            {           
                services.AddSingleton<IKissProxyRoute>((x) => {
                    var zgwTokenProvider = x.GetRequiredService<ZgwTokenProvider>();
                    return new DocumentenProxyConfig(baseUrl, zgwTokenProvider);
                });

                return services;
            }
        }

        public sealed class DocumentenProxyConfig : IKissProxyRoute
        { 
            private readonly ZgwTokenProvider _zgwTokenProvider;

            public DocumentenProxyConfig(string destination , ZgwTokenProvider zgwTokenProvider)
            {
                Destination = destination;
                _zgwTokenProvider = zgwTokenProvider;     
            }

            public string Route => "documenten";
            public string Destination { get; }

            public ValueTask ApplyRequestTransform(RequestTransformContext context)
            {
                var userId = context.HttpContext.User?.FindFirstValue(JwtClaimTypes.PreferredUserName);
                var userRepresentation = context.HttpContext.User?.Identity?.Name;

                var token = _zgwTokenProvider.GenerateToken(userId, userRepresentation);

                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return new();
            }

         
        }
    }

}
