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

        private readonly string _identifier;


        public GetNietNatuurlijkPersoonIdentifier(IConfiguration configuration)
        {
            _identifier = configuration != null && !string.IsNullOrWhiteSpace(configuration["NIETNATUURLIJKPERSOONIDENTIFIER"])
                ? configuration["NIETNATUURLIJKPERSOONIDENTIFIER"]
                : "rsin";
        }

        //Afhanekelijk van de gebruikte bron (openklant of e-Suite)
        //moet men in kunnen stellen welk gegeven gebruikt wordt om 
        //Kvk gegevens aan klanten te koppelen
        //standaard wordt daar vestigingsnummer voor gebruikt,
        //maar voor niet natuurlijke personen moet men rsin of kvknummer kunnen gebruiken
        //openklant accepteert geen kvknummer, de e-Suite kent geen rsin         
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {            
           return Ok(new NietNatuurlijkPersoonIdentifierModel(_identifier));
        }


    }

    public record NietNatuurlijkPersoonIdentifierModel(string NietNatuurlijkPersoonIdentifier);
}
