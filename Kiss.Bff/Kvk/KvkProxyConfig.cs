using Kiss.Bff;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KvkExtensions
    {
        public static IServiceCollection AddKvk(this IServiceCollection services, string baseUrl, string apiKey)
        {
            services.AddSingleton<IKissProxyRoute>(new KvkProxyConfig(baseUrl, apiKey));
            return services;
        }
    }

    public sealed class KvkProxyConfig : IKissProxyRoute
    {
        private readonly string _apiKey;

        public KvkProxyConfig(string destination, string apiKey)
        {
            Destination = destination;
            _apiKey = apiKey;
        }

        public string Route => "kvk";
        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Add("apikey", _apiKey);
            return new();
        }
    }
}
