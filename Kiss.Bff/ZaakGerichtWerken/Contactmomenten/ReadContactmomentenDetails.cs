using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [ApiController]
    public class ReadContactmomentenDetails : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ReadContactmomentenDetails(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/contactmomentdetails")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public async Task<IActionResult> Get([FromQuery] string id, CancellationToken token)
        {
            var contactmoment = await _db.ContactMomentDetails
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contactmoment == null)
            {
                return NotFound();
            }

            return Ok(contactmoment);
        }

        [HttpGet("/api/contactmomentendetails")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public IActionResult Get()
        {
            var contactmomenten = _db.ContactMomentDetails
                .OrderByDescending(x => x.Startdatum)
                .Take(10000)
                .AsAsyncEnumerable();

            return Ok(contactmomenten);
        }
    }
}
