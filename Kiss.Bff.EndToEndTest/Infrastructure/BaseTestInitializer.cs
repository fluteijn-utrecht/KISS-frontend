using Microsoft.Extensions.Configuration;


namespace Kiss.Bff.EndToEndTest
{
    [TestClass]
    public class BaseTestInitializer : PageTest
    {
        private const string StoragePath = "./auth.json";

        private static readonly IConfiguration s_configuration = new ConfigurationBuilder()
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
            await Context.StorageStateAsync(new() { Path = StoragePath });
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
                BaseURL = GetRequiredConfig("TestSettings:TEST_BASE_URL"),
                // save auth state so we don't need to log in in every single test
                StorageStatePath = File.Exists(StoragePath) ? StoragePath : null,
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
