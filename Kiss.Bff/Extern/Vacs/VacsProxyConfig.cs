using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Vacs
{
    public static class VacsExtensions
    {
        public static IServiceCollection AddVacsProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string typeVersion)
            => services.AddSingleton(new VacsProxyConfig(destination, token, objectTypeUrl, typeVersion))
            .AddSingleton<IKissProxyRoute>(s => s.GetRequiredService<VacsProxyConfig>());
    }

    public class VacsProxyConfig : IKissProxyRoute
    {
        public VacsProxyConfig(string destination, string token, string objectTypeUrl, string typeVersion)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            TypeVersion = typeVersion ?? "1";
            _token = token;
        }

        public string Route => "vacs";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }
        public string TypeVersion { get; }

        private readonly string _token;

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
            headers.Authorization = new AuthenticationHeaderValue("Token", _token);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
