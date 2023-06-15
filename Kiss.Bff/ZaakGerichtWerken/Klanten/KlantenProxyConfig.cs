using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using IdentityModel;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.ZaakGerichtWerken.Klanten
{
    public static class KlantenProxyExtensions
    {
        public static IServiceCollection AddKlantenProxy(this IServiceCollection services, string destination, string clientId, string apiKey)
        {
            var tokenProvider = new ZgwTokenProvider(apiKey, clientId);
            var config = new KlantenProxyConfig(destination, tokenProvider);
     

            services.AddSingleton<IKissProxyRoute>(config);
            return services;
        }
    }

    public class KlantenProxyConfig : IKissProxyRoute
    {
        private readonly ZgwTokenProvider _tokenProvider;

        public KlantenProxyConfig(string destination, ZgwTokenProvider tokenProvider)
        {
            Destination = destination;
            _tokenProvider = tokenProvider;
        }

        public string Route => "klanten";

        public string Destination { get; }

        

   
        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var userId = context.HttpContext.User?.FindFirstValue(JwtClaimTypes.PreferredUserName);
            var userRepresentation = context.HttpContext.User?.Identity?.Name;

            var token = _tokenProvider.GenerateToken(userId, userRepresentation);

            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //context.Query.Collection["klant"] = "sdfsdfsdfd";

            var klantQuerystringParams = context.Query.Collection;
         //   context.ProxyRequest.RequestUri

            return new();
        }
    }
}

