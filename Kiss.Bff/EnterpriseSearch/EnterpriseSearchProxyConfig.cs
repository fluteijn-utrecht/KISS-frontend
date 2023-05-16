﻿using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.EnterpriseSearch
{
    public static class EnterpriseSearchExtensions
    {
        public static IServiceCollection AddEnterpriseSearch(this IServiceCollection services, string baseUrl, string apiKey)
        {
            services.AddSingleton<IKissProxyRoute>(new EnterpriseSearchProxyConfig(baseUrl, apiKey));
            return services;
        }
    }

    public class EnterpriseSearchProxyConfig : IKissProxyRoute
    {
        private readonly string _apiKey;

        public EnterpriseSearchProxyConfig(string destination, string apiKey)
        {
            Destination = destination;
            _apiKey = apiKey;
        }

        public string Route => "enterprisesearch";

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            return new();
        }
    }
}
