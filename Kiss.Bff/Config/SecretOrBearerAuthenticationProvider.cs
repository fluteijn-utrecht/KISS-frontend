using System.Net.Http.Headers;
using System.Security.Claims;

namespace Kiss.Bff.Config
{
    public class SecretOrBearerAuthenticationProvider
    {
        private readonly string _clientSecret;
        private readonly ZgwTokenProvider? _tokenProvider;

        public SecretOrBearerAuthenticationProvider(string clientSecret, string? clientId)
        {
            _clientSecret = clientSecret;
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                _tokenProvider = new ZgwTokenProvider(clientId, clientSecret);
            }
        }

        public void SetAuthenticationHeader(HttpRequestHeaders headers, ClaimsPrincipal user)
        {
            if (_tokenProvider == null)
            {
                headers.Authorization = new AuthenticationHeaderValue("Token", _clientSecret);
                return;
            }

            var token = _tokenProvider.GenerateToken(user);
            headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
