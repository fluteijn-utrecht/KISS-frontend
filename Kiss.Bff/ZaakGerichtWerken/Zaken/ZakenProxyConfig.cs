using System.Net.Http.Headers;
using System.Security.Claims;
using IdentityModel;
using Kiss.Bff.ZaakGerichtWerken;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Zaken
{
    public static class ZakenExtensions
    {
        public static IServiceCollection AddZaken(this IServiceCollection services, string baseUrl)
        {
            services.AddSingleton<IKissProxyRoute>((x) =>
            {
                var zgwToken = x.GetRequiredService<ZgwTokenProvider>();
                return new ZakenProxyConfig(baseUrl, zgwToken);
            });

            return services;
        }
    }

    public sealed class ZakenProxyConfig : IKissProxyRoute
    {
        private const string DefaultCrs = "EPSG:4326";
        private readonly ZgwTokenProvider _zgwTokenProvider;

        public ZakenProxyConfig(string destination, ZgwTokenProvider zgwTokenProvider)
        {
            Destination = destination;
            _zgwTokenProvider = zgwTokenProvider;
        }

        public string Route => "zaken";
        public string Destination { get; }

        //documentatie: https://open-zaak.readthedocs.io/en/latest/client-development/authentication.html
        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var token = _zgwTokenProvider.GenerateToken(context.HttpContext.User);

            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            context.ProxyRequest.Headers.Add("Content-Crs", DefaultCrs);
			context.ProxyRequest.Headers.Add("Accept-Crs", DefaultCrs); //voorlopig eignelijk niet nodig. wordt pas relevant wanneer we geografische coordinaten gaan opvragen

			return new();
        }

    }

}
