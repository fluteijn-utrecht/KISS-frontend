﻿using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("build-number")]
        public IActionResult GetBuildNumber()
        {
            var buildNumber = _configuration["RELEASE_BUILD_NUMBER"] ?? "N/A";
            return Ok(new { buildNumber });
        }
    }
}
