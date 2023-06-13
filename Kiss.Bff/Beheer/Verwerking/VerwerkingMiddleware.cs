using Kiss.Bff.Beheer.Data;

namespace Kiss.Bff.Beheer.Verwerking
{
    public static class VerwerkingMiddleware
    {
        public static IServiceCollection AddVerwerkingMiddleware(this IServiceCollection services) => services.AddScoped<IKissHttpClientMiddleware, VerwerkingHandlerWrapper>();

        private class VerwerkingHandlerWrapper : IKissHttpClientMiddleware
        {
            private readonly BeheerDbContext _db;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public VerwerkingHandlerWrapper(BeheerDbContext db, IHttpContextAccessor httpContextAccessor)
            {
                _db = db;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<HttpResponseMessage> SendAsync(SendRequestMessageAsync next, HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var result = await next(request, cancellationToken);

                if (result.IsSuccessStatusCode)
                {
                    var context = _httpContextAccessor.HttpContext;
                    var cluster = context?.GetReverseProxyFeature()?.Cluster?.Config?.ClusterId;

                    // no need to log calls to elasticsearch
                    if (cluster == null || cluster == EnterpriseSearchProxyConfig.ROUTE)
                    {
                        return result;
                    }

                    var apiEndpoint = new UriBuilder(request.RequestUri!)
                    {
                        // query string could contain sensitive data
                        Query = string.Empty
                    }.ToString();

                    var userId = context?.User.GetId();
                    var logging = new VerwerkingsLog { ApiEndpoint = apiEndpoint, Method = request.Method.Method, UserId = userId };
                    await _db.AddAsync(logging, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return result;
            }
        }
    }
}
