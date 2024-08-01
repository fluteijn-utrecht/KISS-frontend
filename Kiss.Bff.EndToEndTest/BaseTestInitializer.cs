using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;


namespace PlaywrightTests
{
    [TestClass]
    public class BaseTestInitializer
    {
        protected IPlaywright Playwright;
        protected IBrowser Browser;
        protected IBrowserContext Context;
        protected IPage Page;
        protected IPage FrontendPage;
        protected AzureAdLoginHelper LoginHelper;
        protected IConfiguration Configuration;

        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            try
            {
                Configuration = new ConfigurationBuilder()
                    .AddUserSecrets<BaseTestInitializer>()
                    .AddEnvironmentVariables()      
                    .Build();

                var username = GetRequiredConfig(Configuration, "TestSettings:TEST_USERNAME");
                var password = GetRequiredConfig(Configuration, "TestSettings:TEST_PASSWORD");
                var totpSecret = GetRequiredConfig(Configuration, "TestSettings:TEST_TOTP_SECRET");
                var isHeadless = GetRequiredConfig(Configuration, "TestSettings:TEST_HEADLESS");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(totpSecret))
                {
                    throw new InvalidOperationException("One or more required settings are missing from appsettings.json");
                }

                Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = bool.Parse(isHeadless)
                });
                Context = await Browser.NewContextAsync();
                Page = await Context.NewPageAsync();
                LoginHelper = new AzureAdLoginHelper(Page, username, password, totpSecret);

                await LoginHelper.LoginAsync();
            }
            catch
            {
                throw;
            }
           
        }

        [TestCleanup]
        public virtual async Task TestCleanup()
        {
            await Context.CloseAsync();
            await Browser.CloseAsync();
            Playwright?.Dispose();
        }

        protected virtual async Task<IPage> OpenNewPageAsync()
        {
            return await Context.NewPageAsync();
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
