using System.Net.Http.Headers;
using System.Security.Claims;
using Kiss.Bff.Config;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Groepen
{
    public static class GroepenExtensions
    {
        public static IServiceCollection AddGroepenProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string? clientId)
            => services.AddSingleton<IKissProxyRoute>(new GroepenProxyConfig(destination, token, objectTypeUrl, clientId));
    }

    public class GroepenProxyConfig : IKissProxyRoute
    {
        private readonly SecretOrBearerAuthenticationProvider _auth;

        public GroepenProxyConfig(string destination, string token, string objectTypeUrl, string? clientId)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            _auth = new SecretOrBearerAuthenticationProvider(token, clientId);
        }

        public string Route => "groepen";

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
