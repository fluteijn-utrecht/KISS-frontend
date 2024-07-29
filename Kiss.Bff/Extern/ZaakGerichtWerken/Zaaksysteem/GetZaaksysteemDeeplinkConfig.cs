using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    public record ZaaksysteemDeeplinkConfig(string BaseUrl, string IdProperty);

    [ApiController]
    public class GetZaaksysteemDeeplinkConfig : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;

        public GetZaaksysteemDeeplinkConfig(IEnumerable<ZaaksysteemConfig> configs)
        {
            _configs = configs;
        }

        [HttpGet("api/zaaksysteem/deeplinkconfig")]
        public IActionResult Get([FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId)
        {
            var config = _configs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();

            if (string.IsNullOrWhiteSpace(config?.DeeplinkBaseUrl)
                || string.IsNullOrWhiteSpace(config?.DeeplinkProperty))
            {
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Stuur een ZaaksysteemId mee waarvoor in KISS een deeplinkUrl en een deeplinkPropery is geconfigureerd",
                    statusCode: 400
                );
            }

            var result = new ZaaksysteemDeeplinkConfig(
                config.DeeplinkBaseUrl,
                config.DeeplinkProperty);

            return Ok(result);
        }
    }
}
