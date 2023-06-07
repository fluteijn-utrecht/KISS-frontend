using System.Net.Http.Headers;
using System.Security.Claims;
using IdentityModel;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    public static class ContactmomentenProxyExtensions
    {
        public static IServiceCollection AddContactmomentenProxy(this IServiceCollection services, string destination, string clientId, string apiKey)
        {
            var tokenProvider = new ZgwTokenProvider(apiKey, clientId);
            var config = new ContactmomentenProxyConfig(destination, tokenProvider);
            services.AddSingleton<IKissProxyRoute>(config);
            return services;
        }
    }

    public class ContactmomentenProxyConfig : IKissProxyRoute
    {
        private readonly ZgwTokenProvider _tokenProvider;

        public ContactmomentenProxyConfig(string destination, ZgwTokenProvider tokenProvider)
        {
            Destination = destination;
            _tokenProvider = tokenProvider;
        }

        public string Route => "contactmomenten";

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var userId = context.HttpContext.User?.FindFirstValue(JwtClaimTypes.PreferredUserName);
            var userRepresentation = context.HttpContext.User?.Identity?.Name;

            var token = _tokenProvider.GenerateToken(userId, userRepresentation);

            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new();
        }
    }
}

