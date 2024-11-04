using System.Globalization;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    [ApiController]
    public class ContactmomentDetailsRapportageOverzicht : ControllerBase
    {
        private readonly BeheerDbContext _db;
        private const int MaxPageSize = 5000;

        public ContactmomentDetailsRapportageOverzicht(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/contactmomentendetails")]
        [Authorize(Policy = Policies.ExternSysteemPolicy)]
        public async Task<IActionResult> Get(
        [FromQuery] string from,
        [FromQuery] string to,
        CancellationToken token,
        [FromQuery] int pageSize = 5000,
        [FromQuery] int page = 1)
        {
            if (!DateTime.TryParseExact(from, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var fromDate) ||
                !DateTime.TryParseExact(to, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var toDate))
            {
                return BadRequest("Invalid date format. Use ISO 8601 format (yyyy-MM-ddTHH:mm:ssZ).");
            }

            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                return BadRequest($"Page size must be between 1 and {MaxPageSize}.");
            }

            var contactmomenten = await _db.ContactMomentDetails
                .Where(x => x.Startdatum >= fromDate && x.Startdatum <= toDate)
                .OrderByDescending(x => x.Startdatum)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ContactmomentDetailsModel(
                    x.Id,
                    x.Einddatum,
                    x.EmailadresKcm,
                    x.Gespreksresultaat,
                    x.SpecifiekeVraag,
                    x.Startdatum,
                    x.VerantwoordelijkeAfdeling,
                    x.Vraag,
                    x.Bronnen.Select(b => new BronModel(
                        b.Soort,
                        b.Titel,
                        b.Url))
                    ))
                .ToListAsync(token);

            return Ok(contactmomenten);
        }

    }
}
