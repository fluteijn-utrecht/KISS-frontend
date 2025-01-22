using System.Net.Http.Headers;
using Kiss.Bff.ZaakGerichtWerken;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Medewerkers
{
    public static class MedewerkersExtensions
    {
        public static IServiceCollection AddMedewerkersProxy(this IServiceCollection services, string destination, string token)
            => services.AddSingleton<IKissProxyRoute>(new MedewerkersProxyConfig(destination, token));
    }

    public class MedewerkersProxyConfig : IKissProxyRoute
    {
        private readonly AuthenticationHeaderProvider _authHeaderProvider;
     
        public MedewerkersProxyConfig(string destination, string token)
        {
            Destination = destination;

            _authHeaderProvider = new AuthenticationHeaderProvider(token, string.Empty, string.Empty);           
        }

        public string Route => "medewerkers";

        public string Destination { get; }


        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            ApplyHeaders(context.ProxyRequest.Headers, context.HttpContext.User);
            var request = context.HttpContext.Request;
            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;

            return new();
        }

        public void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            _authHeaderProvider.ApplyAuthorizationHeader(headers, user);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
