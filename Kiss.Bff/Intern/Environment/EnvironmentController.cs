using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Kiss.Bff.Extern.ZaakGerichtWerken.KlantContacten;

namespace Kiss.Bff.Intern.Environment
{
    [Route("api/environment")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly KlantContactConfig _config;
        private readonly IConfiguration _configuration;

        public EnvironmentController(IOptions<KlantContactConfig> config, IConfiguration configuration)
        {
            _config = config.Value;
            _configuration = configuration;
        }

        [HttpGet("use-klantinteracties")]
        public IActionResult GetUseKlantInteracties()
        {
            var useKlantInteracties = _configuration["USE_KLANTINTERACTIES"] ?? "false";
            return Ok(new { useKlantInteracties = bool.Parse(useKlantInteracties) });
        }

        [HttpGet("use-vacs")]
        public IActionResult GetUseVacs()
        {
            return bool.TryParse(_configuration["USE_VACS"] ?? "false", out var useVacs) ?
                (IActionResult)Ok(new { useVacs }) : Ok(new { useVacs = false });
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

        [HttpGet("default-register")]
        public IActionResult GetDefaultRegister()
        {
            // Gebruik de geconfigureerde klantcontactregisters
            var defaultRegister = _config.Registers
                .Select((register, index) => new { Index = index, register.IsDefault })
                .FirstOrDefault(x => x.IsDefault);

            if (defaultRegister == null)
            {
                return NotFound(new { message = "Geen default register gevonden." });
            }

            return Ok(new { defaultRegister = defaultRegister.Index });
        }

        // Kan later meerdere checks bevatten
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            // Haal de status op uit de configuratie
            var contactRegisterStatus = _configuration["contactRegisterStatus"] ?? "OK";
            return Ok(new { status = contactRegisterStatus });
        }
    }
}
