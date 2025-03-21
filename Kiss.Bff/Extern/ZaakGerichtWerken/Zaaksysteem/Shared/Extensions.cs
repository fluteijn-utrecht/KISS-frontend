﻿using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared
{
    public static class Extensions
    {
        private const string DefaultCrs = "EPSG:4326";

        public static IServiceCollection AddZaaksystemen(this IServiceCollection services, IConfiguration configuration)
        {
            foreach (var zaaksysteem in GetZaakSysteemConfigs(configuration))
            {
                services.AddSingleton(zaaksysteem);
            }
            return services;
        }

        public static IEnumerable<ZaaksysteemConfig> FilterByZaakSysteemId(this IEnumerable<ZaaksysteemConfig> configs, string? zaaksysteemId) => configs
            .Where(x =>
                string.IsNullOrWhiteSpace(zaaksysteemId)
                || x.BaseUrl.AsSpan().TrimEnd('/').Equals(
                        zaaksysteemId.AsSpan().TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase
            ));

        public static void ApplyZaaksysteemHeaders(this HttpRequestHeaders headers, ZaaksysteemConfig config, ClaimsPrincipal user)
        {
            var zgwTokenProvider = new ZgwTokenProvider(config.Secret, config.ClientId);
            var token = zgwTokenProvider.GenerateToken(user);

            headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            headers.Add("Content-Crs", DefaultCrs);
            headers.Add("Accept-Crs", DefaultCrs); //voorlopig eignelijk niet nodig. wordt pas relevant wanneer we geografische coordinaten gaan opvragen

        }

        private static IEnumerable<ZaaksysteemConfig> GetZaakSysteemConfigs(IConfiguration configuration)
        {
            // single zaaksysteem
            if (configuration["ZAKEN_BASE_URL"] is string baseUrl
                && configuration["ZAKEN_API_CLIENT_ID"] is string clientId
                && configuration["ZAKEN_API_KEY"] is string apiKey)
            {
                yield return new(baseUrl, clientId, apiKey, configuration["ZAAKSYSTEEM_DEEPLINK_URL"], configuration["ZAAKSYSTEEM_DEEPLINK_PROPERTY"], configuration["NIETNATUURLIJKPERSOONIDENTIFIER"]);
            }

            // multiple zaaksystemen
            var configs = configuration.GetSection("ZAAKSYSTEEM")?.Get<IEnumerable<Dictionary<string, string>>>();
            if (configs == null) yield break;

            foreach (var item in configs)
            {
                if (item.TryGetValue("BASE_URL", out var itemBaseUrl)
                    && item.TryGetValue("API_CLIENT_ID", out var itemClientId)
                    && item.TryGetValue("API_KEY", out var itemApiKey))
                {
                    var deeplinkUrl = item.GetValueOrDefault("DEEPLINK_URL");
                    var deeplinkProperty = item.GetValueOrDefault("DEEPLINK_PROPERTY");
                    var nnpId = item.GetValueOrDefault("NIETNATUURLIJKPERSOONIDENTIFIER");
                    yield return new(itemBaseUrl, itemClientId, itemApiKey, deeplinkUrl, deeplinkProperty, nnpId);
                }
            }
        }

        public static HttpClient CreateClient(this IHttpClientFactory factory, ZaaksysteemConfig config, ClaimsPrincipal user)
        {
            var client = factory.CreateClient();
            client.BaseAddress = new(config.BaseUrl);
            client.DefaultRequestHeaders.ApplyZaaksysteemHeaders(config, user);
            return client;
        }
    }
}
