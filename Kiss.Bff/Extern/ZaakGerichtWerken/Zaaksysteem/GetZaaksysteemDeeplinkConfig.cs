using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    public record ZaaksysteemDeeplinkConfig(string BaseUrl, string IdProperty);

    [ApiController]
    public class GetZaaksysteemDeeplinkConfig : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;
        private readonly ILogger<GetZaaksysteemDeeplinkConfig> _logger;

        public GetZaaksysteemDeeplinkConfig(IEnumerable<ZaaksysteemConfig> configs, ILogger<GetZaaksysteemDeeplinkConfig> logger)
        {
            _configs = configs;
            _logger = logger;
        }

        [HttpGet("api/zaaksysteem/deeplinkconfig")]
        public IActionResult Get([FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId)
        {
            var config = _configs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();

            if (string.IsNullOrWhiteSpace(config?.DeeplinkBaseUrl)
                || string.IsNullOrWhiteSpace(config?.DeeplinkProperty))
            {
                _logger.LogError("Geen deeplink base url en property gevonden voor ZaaksysteemId {ZaaksysteemId}", zaaksysteemId);
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Geen deeplink base url en property gevonden voor ZaaksysteemId " + zaaksysteemId,
                    statusCode: 500
                );

            }

            var result = new ZaaksysteemDeeplinkConfig(
                config.DeeplinkBaseUrl,
                config.DeeplinkProperty);

            return Ok(result);
        }
    }
}
