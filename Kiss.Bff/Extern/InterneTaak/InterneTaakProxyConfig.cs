using System.Net.Http.Headers;
using Kiss.Bff.ZaakGerichtWerken.Klanten;
using Kiss.Bff.ZaakGerichtWerken;
using Yarp.ReverseProxy.Transforms;
using Microsoft.AspNetCore.DataProtection;
using Kiss.Bff.Afdelingen;

namespace Kiss.Bff.InterneTaak
{
    public static class InterneTaakExtensions
    {
        public static IServiceCollection AddInterneTaakProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
            => services.AddSingleton(new InterneTaakProxyConfig(destination, token, objectTypeUrl, clientId, clientSecret))
            .AddSingleton<IKissProxyRoute>(s => s.GetRequiredService<InterneTaakProxyConfig>());
    }

    public class InterneTaakProxyConfig : IKissProxyRoute
    {
        public InterneTaakProxyConfig(string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;

            _authHeaderProvider = new AuthenticationHeaderProvider(token, clientId, clientSecret);
        }

        public string Route => "internetaak";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }

        private readonly AuthenticationHeaderProvider _authHeaderProvider;

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            ApplyHeaders(context.ProxyRequest.Headers, context.HttpContext.User);
            var request = context.HttpContext.Request;
            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;
            if (request.Method == HttpMethods.Get && isObjectsEndpoint)
            {
                context.Query.Collection["type"] = new(ObjectTypeUrl);
            }
            return new();
        }

        public void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            _authHeaderProvider.ApplyAuthorizationHeader(headers, user);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
