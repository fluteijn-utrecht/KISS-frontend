using Kiss.Bff;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Kiss.Bff
{
    public interface IKissProxyRoute
    {
        string Route { get; }
        string Destination { get; }

        ValueTask ApplyRequestTransform(RequestTransformContext context);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KissProxyExtensions
    {
        public static IServiceCollection AddKissProxy(this IServiceCollection services)
        {
            services.AddReverseProxy();
            services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();
            services.AddSingleton<ITransformProvider, KissTransformProvider>();
            return services;
        }

        public static IEndpointRouteBuilder MapKissProxy(this IEndpointRouteBuilder builder)
        {
            builder.MapReverseProxy();
            return builder;
        }
    }

    public class KissTransformProvider : ITransformProvider
    {
        private readonly IKissProxyRoute[] _proxyRoutes;

        public KissTransformProvider(IEnumerable<IKissProxyRoute> proxyRoutes)
        {
            _proxyRoutes = proxyRoutes.ToArray();
        }

        public void Apply(TransformBuilderContext context)
        {
            var match = _proxyRoutes.FirstOrDefault(x => x.Route == context?.Cluster?.ClusterId);
            if (match != null)
            {
                context.AddRequestTransform((ctx) => match.ApplyRequestTransform(ctx));
            }
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
        }

        public void ValidateRoute(TransformRouteValidationContext context)
        {
        }
    }

    public class ProxyConfigProvider : IProxyConfigProvider
    {
        private readonly SimpleProxyConfig _config;

        public ProxyConfigProvider(IEnumerable<IKissProxyRoute> proxyRoutes)
        {
            var routes = proxyRoutes.Select(x => new RouteConfig
            {
                RouteId = x.Route,
                ClusterId = x.Route,
                Match = new RouteMatch { Path = $"/api/{x.Route.Trim('/')}/{{*any}}" },

                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathRemovePrefix"] = $"/api/{x.Route.Trim('/')}",
                    },
                    new Dictionary<string, string>
                    {
                        ["RequestHeaderRemove"] = "Cookie",
                    }
                }
            }).ToArray();

            var clusters = proxyRoutes.Select(x => new ClusterConfig
            {
                ClusterId = x.Route,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    [x.Route] = new DestinationConfig
                    {
                        Address = x.Destination
                    }
                }
            }).ToArray();

            _config = new SimpleProxyConfig(routes, clusters);
        }

        public IProxyConfig GetConfig() => _config;

        private class SimpleProxyConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new();

            public SimpleProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                Routes = routes ?? throw new ArgumentNullException(nameof(routes));
                Clusters = clusters ?? throw new ArgumentNullException(nameof(clusters));
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }
        }
    }
}


