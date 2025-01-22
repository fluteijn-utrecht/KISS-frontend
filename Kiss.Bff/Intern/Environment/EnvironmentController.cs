using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Intern.Environment
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

        [HttpGet("use-klantinteracties")]
        public IActionResult GetUseKlantIneracties()
        {
            var useKlantInteracties = _configuration["USE_KLANTINTERACTIES"] ?? "false";
            return Ok(new { useKlantInteracties = bool.Parse(useKlantInteracties) });
        }

        [HttpGet("use-vacs")]
        public IActionResult GetUseVacs()
        {
            return bool.TryParse(_configuration["USE_VACS"] ?? "false", out var useVacs) ?
                (IActionResult) Ok(new { useVacs }) : Ok(new { useVacs = false });
        }

        [HttpGet("use-medewerkeremail")]
        public IActionResult GetUseMedewerkerEmail()
        {
            return bool.TryParse(_configuration["USE_MEDEWERKEREMAIL"] ?? "false", out var useMedewerkeremail) ?
                (IActionResult)Ok(new { useMedewerkeremail }) : Ok(new { useMedewerkeremail = false });
        }

        [HttpGet("versienummer")]
        public IActionResult GetVersienummer()
        {
            var versienummer = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return Ok(new { versienummer });
        }
    }
}
