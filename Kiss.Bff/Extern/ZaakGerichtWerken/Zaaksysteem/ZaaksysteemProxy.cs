using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemProxy : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;
        private readonly ILogger<ZaaksysteemProxy> _logger;

        public ZaaksysteemProxy(
            IEnumerable<ZaaksysteemConfig> zakenProxyConfigs,
            ILogger<ZaaksysteemProxy> logger)
        {
            _configs = zakenProxyConfigs;
            _logger = logger;
        }

        [HttpGet("api/zaken/{**path}")]
        [HttpGet("api/documenten/{**path}")]
        public IActionResult Get(
            string path,
            [FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId)
        {
            var config = _configs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();

            if (config == null)
            {
                _logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}", zaaksysteemId);
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + zaaksysteemId,
                    statusCode: 500
                );
            }

            return new ProxyResult(() =>
            {
                var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.ApplyZaaksysteemHeaders(config, User);
                return message;
            });
        }
    }
}
