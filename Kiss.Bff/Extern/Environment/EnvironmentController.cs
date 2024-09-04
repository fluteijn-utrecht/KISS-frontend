using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Environment
{
    [Route("api/environment")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EnvironmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("use-klantcontacten")]
        public IActionResult GetUseKlantContacten()
        {
            var useKlantContacten = _configuration["USE_KLANTCONTACTEN"] ?? "false";
            return Ok(new { useKlantContacten = bool.Parse(useKlantContacten) });
        }
    }
}
