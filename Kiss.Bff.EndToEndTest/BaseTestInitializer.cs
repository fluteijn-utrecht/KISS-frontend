using Microsoft.Extensions.Configuration;


namespace PlaywrightTests
{
    [TestClass]
    public class BaseTestInitializer : PageTest
    {
        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            var configuration = new ConfigurationBuilder()
                 .AddUserSecrets<BaseTestInitializer>()
                 .AddEnvironmentVariables()
                 .Build();

            var username = GetRequiredConfig(configuration, "TestSettings:TEST_USERNAME");
            var password = GetRequiredConfig(configuration, "TestSettings:TEST_PASSWORD");
            var totpSecret = GetRequiredConfig(configuration, "TestSettings:TEST_TOTP_SECRET");
            var isHeadless = GetRequiredConfig(configuration, "TestSettings:TEST_HEADLESS");
            var baseUrl = GetRequiredConfig(configuration, "TestSettings:TEST_BASE_URL");
            var uri = new Uri(baseUrl);

            var loginHelper = new AzureAdLoginHelper(Page, uri, username, password, totpSecret);
            await loginHelper.LoginAsync();
        }

        private string GetRequiredConfig(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"'{key}' is missing from the configuration");
            }
            return value;
        }
    }
}
