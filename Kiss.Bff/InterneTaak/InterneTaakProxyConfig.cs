using System.Net.Http.Headers;
using System.Security.Claims;
using Kiss.Bff.Config;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.InterneTaak
{
    public static class InterneTaakExtensions
    {
        public static IServiceCollection AddInterneTaakProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string? clientId)
            => services.AddSingleton(new InterneTaakProxyConfig(destination, token, objectTypeUrl, clientId))
            .AddSingleton<IKissProxyRoute>(s => s.GetRequiredService<InterneTaakProxyConfig>());
    }

    public class InterneTaakProxyConfig : IKissProxyRoute
    {
        private readonly SecretOrBearerAuthenticationProvider _auth;

        public InterneTaakProxyConfig(string destination, string token, string objectTypeUrl, string? clientId)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            _auth = new SecretOrBearerAuthenticationProvider(token, clientId);
        }

        public string Route => "internetaak";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }


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

        public void ApplyHeaders(HttpRequestHeaders headers, ClaimsPrincipal user)
        {
            _auth.SetAuthenticationHeader(headers, user);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
