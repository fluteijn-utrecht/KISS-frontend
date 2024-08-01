using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class DownloadDocument : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;
        private readonly ILogger<DownloadDocument> _logger;

        public DownloadDocument(IEnumerable<ZaaksysteemConfig> configs, ILogger<DownloadDocument> logger)
        {
            _configs = configs;
            _logger = logger;
        }

        [HttpGet("api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/{id:guid}/download")]
        public IActionResult Download(Guid id, [FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId)
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
                var url = config.BaseUrl.TrimEnd('/') + $"/documenten/api/v1/enkelvoudiginformatieobjecten/{id}/download{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.ApplyZaaksysteemHeaders(config, User);
                return message;
            });
        }
    }
}
