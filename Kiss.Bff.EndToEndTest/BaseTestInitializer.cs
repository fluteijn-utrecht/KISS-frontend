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

                var username = Configuration["TestSettings:TEST_USERNAME"];
                var password = Configuration["TestSettings:TEST_PASSWORD"];
                var totpSecret = Configuration["TestSettings:TEST_TOTP_SECRET"];

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(totpSecret))
                {
                    throw new InvalidOperationException("One or more required settings are missing from appsettings.json");
                }

                Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false // Set to true
                });
                Context = await Browser.NewContextAsync();
                Page = await Context.NewPageAsync();
                LoginHelper = new AzureAdLoginHelper(Page,
                    Configuration["TestSettings:TEST_USERNAME"],
                    Configuration["TestSettings:TEST_PASSWORD"],
                    Configuration["TestSettings:TEST_TOTP_SECRET"]);

                await LoginHelper.LoginAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
    }
}
