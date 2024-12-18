using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactverzoekenVragensets
{
    [ApiController]
    [Route("api/contactverzoekvragensets")]
    public class ReadContactverzoekenVragenSets : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ReadContactverzoekenVragenSets(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("{soort}")]
        public async Task<IActionResult> Get(string soort, CancellationToken cancellationToken)
        {
            var contactVerzoekVragenSets = await _db.ContactVerzoekVragenSets.Where(x => x.OrganisatorischeEenheidSoort == soort).ToListAsync(cancellationToken);

            return Ok(contactVerzoekVragenSets);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var contactVerzoekVragenSet = await _db.ContactVerzoekVragenSets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (contactVerzoekVragenSet == null)
            {
                return NotFound();
            }

            return Ok(contactVerzoekVragenSet);
        }

    }
}
