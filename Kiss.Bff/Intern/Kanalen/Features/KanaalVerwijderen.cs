using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Kanalen
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanaalVerwijderen : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public KanaalVerwijderen(BeheerDbContext context)
        {
            _context = context;
        }

        
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken token)
        {
            var item = await _context.Kanalen.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);
            if (item == null)
            {
                return NoContent();
            }

            _context.Kanalen.Remove(item);
            await _context.SaveChangesAsync(token);

            return NoContent();
        }
    }

}
