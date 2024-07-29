using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;

namespace Kiss.Bff.Test.Zaaksysteem
{
    [TestClass]
    public class ZaaksysteemConfigTests
    {
        [TestMethod]
        public void Legacy_environment_variables_are_parsed_as_zaaksysteem_config()
        {
            var services = new ServiceCollection();
            
            services.AddZaaksystemen(CreateConfig(new()
            {
                ["ZAKEN_BASE_URL"] = "LEGACY_URL",
                ["ZAKEN_API_CLIENT_ID"] = "LEGACY_ID",
                ["ZAKEN_API_KEY"] = "LEGACY_KEY",
                ["ZAAKSYSTEEM_DEEPLINK_URL"] = "LEGACY_DEEPLINK",
                ["ZAAKSYSTEEM_DEEPLINK_PROPERTY"] = "LEGACY_PROPERTY",
            }));

            var provider = services.BuildServiceProvider();

            var first = provider.GetServices<ZaaksysteemConfig>().SingleOrDefault(x=> x.BaseUrl == "LEGACY_URL");

            Assert.IsNotNull(first);
            Assert.AreEqual(first.ClientId, "LEGACY_ID");
            Assert.AreEqual(first.Secret, "LEGACY_KEY");
            Assert.AreEqual(first.DeeplinkBaseUrl, "LEGACY_DEEPLINK");
            Assert.AreEqual(first.DeeplinkProperty, "LEGACY_PROPERTY");
        }

        [TestMethod]
        public void New_list_environment_variables_are_parsed_as_zaaksysteem_config()
        {
            var services = new ServiceCollection();

            services.AddZaaksystemen(CreateConfig(new()
            {
                ["ZAAKSYSTEEM__0__BASE_URL"] = "NEW_BASE_URL",
                ["ZAAKSYSTEEM__0__API_CLIENT_ID"] = "NEW_API_CLIENT_ID",
                ["ZAAKSYSTEEM__0__API_KEY"] = "NEW_API_KEY",
                ["ZAAKSYSTEEM__0__DEEPLINK_URL"] = "NEW_DEEPLINK_URL",
                ["ZAAKSYSTEEM__0__DEEPLINK_PROPERTY"] = "NEW_DEEPLINK_PROPERTY",
            }));

            var provider = services.BuildServiceProvider();

            var first = provider.GetServices<ZaaksysteemConfig>().SingleOrDefault(x=> x.BaseUrl == "NEW_BASE_URL");

            Assert.IsNotNull(first);
            Assert.AreEqual(first.ClientId, "NEW_API_CLIENT_ID");
            Assert.AreEqual(first.Secret, "NEW_API_KEY");
            Assert.AreEqual(first.DeeplinkBaseUrl, "NEW_DEEPLINK_URL");
            Assert.AreEqual(first.DeeplinkProperty, "NEW_DEEPLINK_PROPERTY");
        }

        private static IConfiguration CreateConfig(Dictionary<string, string> dict)
        {
            foreach (var (key, value) in dict)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
