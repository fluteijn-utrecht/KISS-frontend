using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    [ApiController]
    public class ReadContactmomentDetails : ControllerBase
    {
        private readonly BeheerDbContext _db;
        private const int MaxPageSize = 5000;

        public ReadContactmomentDetails(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/contactmomentdetails")]
        public async Task<IActionResult> Get([FromQuery] string id, CancellationToken token)
        {
            var contactmoment = await _db.ContactMomentDetails
                .Where(x => x.Id == id)
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
                        b.Url
                    ))
                ))
                .AsSplitQuery()
                .FirstOrDefaultAsync(token);

            if (contactmoment == null)
            {
                return NotFound();
            }

            return Ok(contactmoment);
        }
    }

}
