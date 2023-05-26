using System.Net.Http.Headers;
using Kiss.Bff;
using NuGet.Common;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Zaken
{

    namespace Microsoft.Extensions.DependencyInjection
    {
        public static class ZakenExtensions
        {
            public static IServiceCollection AddZaken(this IServiceCollection services, string baseUrl, string apiKey)
            {
                services.AddSingleton<IKissProxyRoute>(new ZakenProxyConfig(baseUrl, apiKey));
                return services;
            }
        }

        public sealed class ZakenProxyConfig : IKissProxyRoute
        {
            private readonly string _apiKey;

            public ZakenProxyConfig(string destination, string apiKey)
            {
                Destination = destination;
                _apiKey = apiKey;
            }

            public string Route => "zaken";
            public string Destination { get; }

            public ValueTask ApplyRequestTransform(RequestTransformContext context)
            {
                //hier het tijdelijk geldige token uit https://open-zaak.dev.kiss-demo.nl/admin/authorizations/applicatie/1/change/ invullen
                var token = "....";
                //todo vervangen door call die token opvraagt

                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                context.ProxyRequest.Headers.Add("Accept-Crs", "EPSG:4326"); //todo uitzoeken. moet dit?? en moet dit configurabel zijn?
                return new();
            }
        }
    }

}
