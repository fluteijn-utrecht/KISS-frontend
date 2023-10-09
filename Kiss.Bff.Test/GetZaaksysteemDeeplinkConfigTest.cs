using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.Zaaksysteem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class GetZaaksysteemDeeplinkConfigTest
    {
        [TestMethod]
        public void GetConfigReturnsCorrectVariables()
        {
            var baseUrl = Guid.NewGuid().ToString();
            var idProperty = Guid.NewGuid().ToString();

            var dict = new Dictionary<string, string> 
            {
                ["ZAAKSYSTEEM_DEEPLINK_BASE_URL"] = baseUrl,
                ["ZAAKSYSTEEM_DEEPLINK_PROPERTY"] = idProperty,
            };

            var config = new FakeConfig(dict);
            var sut = new GetZaaksysteemDeeplinkConfig(config);
            var result = sut.Get();

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var obj = (result as ObjectResult)!.Value;
            Assert.IsInstanceOfType(obj, typeof(ZaaksysteemDeeplinkConfig));
            var deeplinkConfig = obj as ZaaksysteemDeeplinkConfig;
            Assert.AreEqual(baseUrl, deeplinkConfig!.BaseUrl);
            Assert.AreEqual(idProperty, deeplinkConfig!.IdProperty);
        }

        private class FakeConfig : IConfiguration
        {
            private readonly IReadOnlyDictionary<string, string> _keyValuePairs;

            public FakeConfig(IReadOnlyDictionary<string,string> keyValuePairs)
            {
                _keyValuePairs = keyValuePairs;
            }

            public string this[string key] { get => _keyValuePairs[key]; set => throw new NotImplementedException(); }

            public IEnumerable<IConfigurationSection> GetChildren()
            {
                throw new NotImplementedException();
            }

            public IChangeToken GetReloadToken()
            {
                throw new NotImplementedException();
            }

            public IConfigurationSection GetSection(string key)
            {
                throw new NotImplementedException();
            }
        }
    }
}
