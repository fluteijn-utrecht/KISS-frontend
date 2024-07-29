using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class DownloadDocument : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;

        public DownloadDocument(IEnumerable<ZaaksysteemConfig> configs)
        {
            _configs = configs;
        }

        [HttpGet("api/documenten/documenten/api/v1/enkelvoudiginformatieobjecten/{id:guid}/download")]
        public IActionResult Download(Guid id, [FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId)
        {
            var config = _configs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();
            if (config == null)
            {
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Stuur een ZaaksysteemId mee waarvoor in KISS een Zaaksysteem is geconfigureerd",
                    statusCode: 400
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
