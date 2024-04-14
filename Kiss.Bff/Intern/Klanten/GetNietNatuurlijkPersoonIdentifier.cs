using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Ganss.Xss;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Feedback
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetNietNatuurlijkPersoonIdentifier : ControllerBase
    {
        
        private readonly IConfiguration _configuration;

       
        public GetNietNatuurlijkPersoonIdentifier( IConfiguration configuration)
        {        
            _configuration = configuration;
        }

        //Afhaneklijk van de gebruikte bron (openklant of e-Suite)
        //moet men in kunnen stellen welk gegeven gebruikt wordt om 
        //Kvk gegevens aan klanten te koppelen
        //standaard wordt daar vestigingsnummer voor gebruikt,
        //maar voor niet natuurlijke personen moet men rsin of kvknummer kunnen gebruiken
        //openklant accepteert geen kvknummer, de e-Suite kent geen rsin         
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            //todo: get from configuration
            return Ok(new NietNatuurlijkPersoonIdentifierModel("rsin"));
        }

     
    }

    public record NietNatuurlijkPersoonIdentifierModel(string NietNatuurlijkPersoonIdentifier);
}
