using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Kiss.Bff.Intern.Kanalen.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Kanalen
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanaalToevoegen : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public KanaalToevoegen(BeheerDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public async Task<ActionResult<KanaalToevoegenModel>> Post(KanaalToevoegenModel model, CancellationToken token)
        {
            var entity = new Kanaal
            {
                DateCreated = DateTimeOffset.UtcNow,
                Naam = model.Naam
            };

            await _context.Kanalen.AddAsync(entity, token);
            await _context.SaveChangesAsync(token);

            return NoContent();
            //return CreatedAtAction("KanaalToevoegen", new { id = entity.Id }, model with { Id = entity.Id });
        }
                
    }

    public record KanaalToevoegenModel(Guid Id, string Naam);
}
