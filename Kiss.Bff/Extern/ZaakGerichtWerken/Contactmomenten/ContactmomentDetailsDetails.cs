using System.Globalization;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
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
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contactmoment == null)
            {
                return NotFound();
            }

            return Ok(contactmoment);
        }

        
    }
}
