using System.Net.Http.Headers;
using Kiss.Bff.ZaakGerichtWerken.Klanten;
using Kiss.Bff.ZaakGerichtWerken;
using Yarp.ReverseProxy.Transforms;
using Microsoft.AspNetCore.DataProtection;

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
        private readonly AuthenticationHeaderValue? _authHeader;
        private readonly ZgwTokenProvider? _tokenProvider;

       // Secret instellen in overige obejcten!!!!
       
        public InterneTaakProxyConfig(string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;

            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret) ) {

                _tokenProvider = new ZgwTokenProvider(clientSecret, clientId);               
                return;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                _authHeader = new AuthenticationHeaderValue("Token", token);
                return;
            }

            if (_tokenProvider == null && _authHeader == null)
            {
                throw new Exception("Setting up a proxy for InterneTaak failed. A token or clientId/clientSecret combination should be provided");
            }
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

        public void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {

            if (_authHeader != null)
            {
                headers.Authorization = _authHeader;
            }
            else if(_tokenProvider != null)
            {
                var token = _tokenProvider.GenerateToken(user);
                headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
