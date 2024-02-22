﻿using System.Net.Http.Headers;
using System.Security.Claims;
using IdentityModel;
using Kiss.Bff.Config;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Zaken
{
    public static class DocumentenExtensions
    {
        public static IServiceCollection AddDocumenten(this IServiceCollection services, string baseUrl)
        {
            services.AddSingleton<IKissProxyRoute>((x) =>
            {
                var zgwTokenProvider = x.GetRequiredService<ZgwTokenProvider>();
                return new DocumentenProxyConfig(baseUrl, zgwTokenProvider);
            });

            return services;
        }
    }

    public sealed class DocumentenProxyConfig : IKissProxyRoute
    {
        private readonly ZgwTokenProvider _zgwTokenProvider;

        public DocumentenProxyConfig(string destination, ZgwTokenProvider zgwTokenProvider)
        {
            Destination = destination;
            _zgwTokenProvider = zgwTokenProvider;
        }

        public string Route => "documenten";
        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var token = _zgwTokenProvider.GenerateToken(context.HttpContext.User);

            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new();
        }

    }
}
