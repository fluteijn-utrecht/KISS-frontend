using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Zaaksysteem
{
    [ApiController]
    public class GetZaaksysteemDeeplinkConfig : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetZaaksysteemDeeplinkConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("api/zaaksysteem/deeplinkconfig")]
        public IActionResult Get() => Ok(new ZaaksysteemDeeplinkConfig(_configuration["ZAAKSYSTEEM_DEEPLINK_BASE_URL"], _configuration["ZAAKSYSTEEM_DEEPLINK_PROPERTY"]));
    }

    public record ZaaksysteemDeeplinkConfig(string BaseUrl, string IdProperty);
}
