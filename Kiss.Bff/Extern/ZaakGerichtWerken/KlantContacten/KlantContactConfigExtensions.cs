using Serilog;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.KlantContacten
{
    public static class KlantContactConfigExtensions
    {
        public static IServiceCollection AddKlantContactConfig(this IServiceCollection services, IConfiguration configuration, out string statusMessage)
        {
            var registers = new List<KlantContactRegister>();

            for (int i = 0; i < 10; i++)
            {
                var baseUrl = configuration[$"KLANTCONTACT_{i}_BASE_URL"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    registers.Add(new KlantContactRegister
                    {
                        IsDefault = bool.TryParse(configuration[$"KLANTCONTACT_{i}_IS_DEFAULT"], out var isDefault) && isDefault,
                        BaseUrl = baseUrl,
                        ApiVersion = configuration[$"KLANTCONTACT_{i}_API_VERSION"],
                        ClientId = configuration[$"KLANTCONTACT_{i}_CLIENT_ID"],
                        ClientSecret = configuration[$"KLANTCONTACT_{i}_CLIENT_SECRET"],
                        Token = configuration[$"KLANTCONTACT_{i}_TOKEN"]
                    });
                }
            }

            // Controle op meerdere defaults
            var defaultRegisters = registers.Where(r => r.IsDefault).ToList();
            if (defaultRegisters.Count > 1)
            {
                statusMessage = "FOUT: Meerdere default registers ingesteld.";
            }
            else if (defaultRegisters.Count == 0)
            {
                statusMessage = "WAARSCHUWING: Geen default register ingesteld.";
            }
            else
            {
                statusMessage = "OK";
            }

            services.Configure<KlantContactConfig>(config =>
            {
                config.Registers = registers;
            });

            return services;
        }
    }
}
