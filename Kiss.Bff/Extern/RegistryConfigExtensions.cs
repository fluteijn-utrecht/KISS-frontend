namespace Kiss.Bff.Extern
{
    public static class RegistryConfigExtensions
    {
        public static IServiceCollection AddRegistryConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // first check if we can build the registries from the new environment variable format
            var registries = GetFromNewEnvironmentVariables(configuration).ToList();

            if (registries.Count == 0)
            {
                // otherwise, try the old environment variable format
                registries.AddRange(GetFromOldEnvironmentVariables(configuration));
            }

            // check if we have all the configuration we need
            var statusMessage = Validate(registries);

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                throw new Exception(statusMessage);
            }

            var registryConfig = new RegistryConfig { Systemen = registries };

            services.AddSingleton(registryConfig);

            return services;
        }

        /// <summary>
        /// Get the registry config using the new convention of environment variables <br/>
        /// For example: <br/>
        /// REGISTERS__0__IS_DEFAULT <br/>
        /// REGISTERS__0__KLANTINTERACTIE_CLIENT_ID <br/>
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws when the KLANTINTERACTIE_BASE_URL is missing</exception>
        private static IEnumerable<RegistrySystem> GetFromNewEnvironmentVariables(IConfiguration configuration)
        {
            var configs = configuration.GetSection("REGISTERS")?.Get<IEnumerable<Dictionary<string, string>>>() ?? [];

            foreach (var item in configs)
            {
                string? GetValue(string key) => item.TryGetValue(key, out var value) ? value : default;
                var contactmomentenBaseUrl = GetValue("KLANTINTERACTIE_BASE_URL") ?? throw new Exception("Fout: base url ontbreekt voor klantinteractie");
                var interneTaakBaseUrl = GetValue("INTERNE_TAAK_BASE_URL");
                var interneTaakObjectTypeUrl = GetValue("INTERNE_TAAK_OBJECT_TYPE_URL");

                yield return new RegistrySystem
                {
                    IsDefault = bool.TryParse(GetValue("IS_DEFAULT"), out var isDefault) && isDefault,
                    KlantinteractieVersion = Enum.TryParse<KlantinteractieVersion>(GetValue("KLANTINTERACTIE_API_VERSION"), out var versie)
                        ? versie
                        : KlantinteractieVersion.OpenKlant2,
                    Identifier = contactmomentenBaseUrl,
                    KlantinteractieRegistry = new KlantinteractieRegistry
                    {
                        BaseUrl = contactmomentenBaseUrl,
                        ClientId = GetValue("KLANTINTERACTIE_CLIENT_ID"),
                        ClientSecret = GetValue("KLANTINTERACTIE_CLIENT_SECRET"),
                        Token = GetValue("KLANTINTERACTIE_TOKEN")
                    },
                    InterneTaakRegistry = string.IsNullOrWhiteSpace(interneTaakBaseUrl) || string.IsNullOrWhiteSpace(interneTaakObjectTypeUrl)
                        ? null
                        : new InternetaakRegistry
                        {
                            BaseUrl = interneTaakBaseUrl,
                            ClientId = GetValue("INTERNE_TAAK_CLIENT_ID"),
                            ClientSecret = GetValue("INTERNE_TAAK_CLIENT_SECRET"),
                            Token = GetValue("INTERNE_TAAK_TOKEN"),
                            ObjectTypeUrl = interneTaakObjectTypeUrl,
                            ObjectTypeVersion = GetValue("INTERNE_TAAK_TYPE_VERSION") ?? "1"
                        }
                };
            }
        }

        /// <summary>
        /// Get the registry config using the new convention of environment variables <br/>
        /// For example: <br/>
        /// USE_KLANTINTERACTIES <br/>
        /// CONTACTMOMENTEN_API_CLIENT_ID <br/>
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static IEnumerable<RegistrySystem> GetFromOldEnvironmentVariables(IConfiguration configuration)
        {
            var useKlantInteracties = bool.TryParse(configuration["USE_KLANTINTERACTIES"], out var validBool) && validBool;

            return useKlantInteracties
                ? GetOpenKlant2(configuration)
                : GetOpenKlant1(configuration);
        }

        private static IEnumerable<RegistrySystem> GetOpenKlant1(IConfiguration configuration)
        {
            var contactmomentenBaseUrl = configuration["CONTACTMOMENTEN_BASE_URL"];
            if (!string.IsNullOrWhiteSpace(contactmomentenBaseUrl))
            {
                var interneTaakBaseUrl = configuration["INTERNE_TAAK_BASE_URL"];
                var interneTaakObjectTypeUrl = configuration["INTERNE_TAAK_OBJECT_TYPE_URL"];


                yield return new RegistrySystem
                {
                    Identifier = contactmomentenBaseUrl,
                    IsDefault = true,
                    KlantinteractieVersion = KlantinteractieVersion.OpenKlant1,
                    KlantinteractieRegistry = new KlantinteractieRegistry
                    {
                        BaseUrl = contactmomentenBaseUrl,
                        ClientId = configuration["CONTACTMOMENTEN_API_CLIENT_ID"],
                        ClientSecret = configuration["CONTACTMOMENTEN_API_KEY"]
                    },
                    InterneTaakRegistry = string.IsNullOrWhiteSpace(interneTaakBaseUrl) || string.IsNullOrWhiteSpace(interneTaakObjectTypeUrl)
                    ? null
                    : new InternetaakRegistry
                    {
                        BaseUrl = interneTaakBaseUrl,
                        ClientId = configuration["INTERNE_TAAK_CLIENT_ID"],
                        ClientSecret = configuration["INTERNE_TAAK_CLIENT_SECRET"],
                        Token = configuration["INTERNE_TAAK_TOKEN"],
                        ObjectTypeUrl = interneTaakObjectTypeUrl,
                        ObjectTypeVersion = configuration["INTERNE_TAAK_TYPE_VERSION"] ?? "1"
                    }
                };
            }
        }

        private static IEnumerable<RegistrySystem> GetOpenKlant2(IConfiguration configuration)
        {
            var contactmomentenBaseUrl = configuration["KLANTINTERACTIES_BASE_URL"];
            if (!string.IsNullOrWhiteSpace(contactmomentenBaseUrl))
            {
                yield return new RegistrySystem
                {
                    Identifier = contactmomentenBaseUrl,
                    IsDefault = true,
                    KlantinteractieVersion = KlantinteractieVersion.OpenKlant2,
                    KlantinteractieRegistry = new KlantinteractieRegistry
                    {
                        BaseUrl = contactmomentenBaseUrl,
                        Token = configuration["KLANTINTERACTIES_TOKEN"]
                    }
                };
            }
        }

        /// <summary>
        /// Checks if the configuration of the registries is sufficient
        /// </summary>
        /// <param name="systemen"></param>
        /// <returns>An error message if applicable, or an empty string if all is well</returns>
        private static string Validate(IReadOnlyList<RegistrySystem> systemen)
        {
            foreach (var systeem in systemen)
            {
                // OK2
                if (systeem.KlantinteractieVersion == KlantinteractieVersion.OpenKlant2)
                {
                    if (string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.Token))
                    {
                        return "FOUT: Bij OpenKlant2 moet voor het KlantinteractieRegister een Token geconfigureerd worden.";
                    }
                }
                // OK1
                else
                {
                    if (string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.ClientSecret) || string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.ClientId))
                    {
                        return "FOUT: Bij OpenKlant1 / eSuite moet voor het KlantinteractieRegister een ClientSecret en een ClientId geconfigureerd worden.";
                    }
                    if (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry?.Token)
                        && (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry?.ClientId) || string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry?.ClientSecret)))
                    {
                        return "FOUT: Bij eSuite moet voor het InterneTaakRegister een ClientSecret en een ClientId geconfigureerd worden. Bij OpenKlant1 moet voor het InterneTaakRegister een Token geconfigureerd worden.";
                    }
                    if (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry?.ObjectTypeUrl))
                    {
                        return "FOUT: Bij OpenKlant1 / eSuite moet voor het InterneTaakRegister ObjectTypeUrl geconfigureerd worden.";
                    }
                }
            }

            var defaultRegistersCount = systemen.Where(r => r.IsDefault).Count();

            if (defaultRegistersCount > 1)
            {
                return "FOUT: Meerdere default registers ingesteld.";
            }

            if (defaultRegistersCount == 0)
            {
                return "FOUT: Geen default register ingesteld.";
            }

            return "";
        }
    }
}
