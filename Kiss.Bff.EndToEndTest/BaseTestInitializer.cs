using Kiss.Bff.EndToEndTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Testing.Platform.Configurations;


namespace PlaywrightTests
{
    [TestClass]
    public class BaseTestInitializer : PageTest
    {
        private static readonly Microsoft.Extensions.Configuration.IConfiguration s_configuration = new ConfigurationBuilder()
            .AddUserSecrets<BaseTestInitializer>()
            .AddEnvironmentVariables()
            .Build();

        private static readonly UniqueOtpHelper s_uniqueOtpHelper = new(GetRequiredConfig("TestSettings:TEST_TOTP_SECRET"));

        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            var username = GetRequiredConfig("TestSettings:TEST_USERNAME");
            var password = GetRequiredConfig("TestSettings:TEST_PASSWORD");

            await Context.Tracing.StartAsync(new()
            {
                Title = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}",
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            var loginHelper = new AzureAdLoginHelper(Page, username, password, s_uniqueOtpHelper);
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

        public override BrowserNewContextOptions ContextOptions()
        {
            return new(base.ContextOptions())
            {
                BaseURL = GetRequiredConfig("TestSettings:TEST_BASE_URL")
            };
        }

        private static string GetRequiredConfig(string key)
        {
            var value = s_configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"'{key}' is missing from the configuration");
            }
            return value;
        }
    }
}
