using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.InterneTaak
{
    public static class InterneTaakExtensions
    {
        public static IServiceCollection AddInterneTaakProxy(this IServiceCollection services, string destination, string token) => services.AddSingleton<IKissProxyRoute>(new InterneTaakProxyConfig(destination, token));
        public static void MapInterneTaakObjectTypeUrlEndpoint(this IEndpointRouteBuilder routeBuilder, string interneTaakObjectTypeUrl) => routeBuilder.MapGet("/api/internetaak/objecttypeurl", () => interneTaakObjectTypeUrl);
    }

    public class InterneTaakProxyConfig : IKissProxyRoute
    {
        private readonly AuthenticationHeaderValue _authHeader;

        public InterneTaakProxyConfig(string destination, string token)
        {
            _authHeader = new AuthenticationHeaderValue("Token", token);
            Destination = destination;
        }

        public string Route => "internetaak";

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Authorization = _authHeader;
            return new();
        }
    }
}
