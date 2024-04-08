using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Kanalen
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanaalBewerken : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public KanaalBewerken(BeheerDbContext context)
        {
            _context = context;
        } 

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public async Task<IActionResult> Put(Guid id, KanaalBewerkenModel model, CancellationToken token)
        {
            var entity = await _context.Kanalen.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);
            if (entity == null) return NotFound();

            entity.DateUpdated = DateTimeOffset.UtcNow;
            entity.Naam = model.Naam;

            await _context.SaveChangesAsync(token);
            return NoContent();
        }       
        
    }

    public record KanaalBewerkenModel(Guid Id, string Naam);
}
