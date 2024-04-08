using Kiss.Bff;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HaalCentraalExtensions
    {
        public static IServiceCollection AddHaalCentraal(this IServiceCollection services, string baseUrl, string apiKey)
        {
            services.AddSingleton<IKissProxyRoute>(new HaalCentraalProxyConfig(baseUrl, apiKey));
            return services;
        }
    }

    public class HaalCentraalProxyConfig : IKissProxyRoute
    {
        private readonly string _apiKey;

        public HaalCentraalProxyConfig(string destination, string apiKey)
        {
            Destination = destination;
            _apiKey = apiKey;
        }

        public string Route => "haalcentraal";

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Add("X-API-KEY", _apiKey);
            return new();
        }
    }
}
