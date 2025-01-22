
using System.Collections.Generic;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.KlantContacten
{
    public class KlantContactConfig
    {
        public List<KlantContactRegister> Registers { get; set; } = new();
    }

    public class KlantContactRegister
    {
        public bool IsDefault { get; set; }
        public string BaseUrl { get; set; }
        public string ApiVersion { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Token { get; set; }
    }
}
