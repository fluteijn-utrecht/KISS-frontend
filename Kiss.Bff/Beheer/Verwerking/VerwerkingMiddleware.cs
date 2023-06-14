using System.Security.Claims;
using Kiss.Bff.Beheer.Data;

namespace Kiss.Bff.Beheer.Verwerking
{
    public static class VerwerkingMiddleware
    {
        public static IServiceCollection AddVerwerkingMiddleware(this IServiceCollection services) => services.AddScoped<IKissHttpClientMiddleware, VerwerkingsHttpClientMiddleware>();

        private class VerwerkingsHttpClientMiddleware : IKissHttpClientMiddleware
        {
            private readonly BeheerDbContext _db;
            private readonly ClaimsPrincipal _user;

            public VerwerkingsHttpClientMiddleware(BeheerDbContext db, ClaimsPrincipal user)
            {
                _db = db;
                _user = user;
            }

            public bool IsEnabled(string? clusterId) => clusterId != EnterpriseSearchProxyConfig.ROUTE;

            public async Task<HttpResponseMessage> SendAsync(SendRequestMessageAsync next, HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var result = await next(request, cancellationToken);

                if (!result.IsSuccessStatusCode) return result;

                var apiEndpoint = new UriBuilder(request.RequestUri!)
                {
                    // query string could contain sensitive data
                    Query = string.Empty
                }.Uri.ToString();

                var userId = _user?.GetId();
                var logging = new VerwerkingsLog { ApiEndpoint = apiEndpoint, Method = request.Method.Method, UserId = userId };
                await _db.AddAsync(logging, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                return result;
            }
        }
    }
}
