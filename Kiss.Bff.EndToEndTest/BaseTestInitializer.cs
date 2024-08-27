using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;


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
            var baseUrl = GetRequiredConfig(configuration, "TestSettings:TEST_BASE_URL");
            var uri = new Uri(baseUrl);

            await Context.Tracing.StartAsync(new()
            {
                Title = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}",
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            var loginHelper = new AzureAdLoginHelper(Page, uri, username, password, totpSecret);
            await loginHelper.LoginAsync();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            var options = TestContext.CurrentTestOutcome != UnitTestOutcome.Passed 
                ? new TracingStopOptions
                {
                    Path = Path.Combine(
                        Environment.CurrentDirectory,
                        "playwright-traces",
                        $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip"
                    )
                } 
                : null;

            await Context.Tracing.StopAsync(options);
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
