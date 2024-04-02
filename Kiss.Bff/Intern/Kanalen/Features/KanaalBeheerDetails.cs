using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Kanalen
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanaalBeheerDetails : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public KanaalBeheerDetails(BeheerDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KanaalOverzichtModel>> Get(Guid id)
        {
            var result = await _context
               .Kanalen
               .AsNoTracking()
               .FirstOrDefaultAsync( x=>x.Id == id );

            return result == null
              ? NotFound()
              : new KanaalOverzichtModel(result.Id, result.Naam);
        }

    }

    public record KanaalOverzichtModel(Guid Id, string Naam);
}
