using System.Text.Json;
using System.Threading;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactverzoeken
{
    [ApiController]
    [Authorize(Policy = Policies.RedactiePolicy)]
    public class WriteContactverzoekenVragenSets : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public WriteContactverzoekenVragenSets(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpPost("/api/contactverzoekvragensets")]
        public async Task<IActionResult> Post(ContactVerzoekVragenSet model, CancellationToken cancellationToken)
        {
            await _db.AddAsync(model, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        [HttpPut("/api/contactverzoekvragensets/{id:int}")]
        public async Task<IActionResult> Put(int id, ContactVerzoekVragenSet model, CancellationToken cancellationToken)
        {
            var contactVerzoekVragenSet = await _db.ContactVerzoekVragenSets.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (contactVerzoekVragenSet == null)
            {
                return NotFound();
            }

            contactVerzoekVragenSet.Titel = model.Titel;
            contactVerzoekVragenSet.JsonVragen = model.JsonVragen;
            contactVerzoekVragenSet.AfdelingId = model.AfdelingId;
            contactVerzoekVragenSet.AfdelingNaam = model.AfdelingNaam;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        [HttpDelete("/api/contactverzoekvragensets/{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var contactVerzoekVragenSet = await _db.ContactVerzoekVragenSets.FirstOrDefaultAsync(e => e.Id == id);

            if (contactVerzoekVragenSet == null)
            {
                return NotFound();
            }

            _db.ContactVerzoekVragenSets.Remove(contactVerzoekVragenSet);
            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}
