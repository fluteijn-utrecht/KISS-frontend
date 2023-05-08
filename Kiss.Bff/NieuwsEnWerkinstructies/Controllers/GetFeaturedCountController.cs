using Kiss.Bff.NieuwsEnWerkinstructies.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Controllers
{
    [ApiController]
    public class GetFeaturedCountController : ControllerBase
    {
        private readonly NieuwsEnWerkinstructiesDbContext _context;

        public GetFeaturedCountController(NieuwsEnWerkinstructiesDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/berichten/featuredcount")]
        public async Task<IActionResult> GetCount(CancellationToken token)
        {
            var count = await _context.Berichten.Where(x => x.IsBelangrijk).CountAsync(token);
            return Ok(new FeaturedCount(count));
        }
    }

    public record struct FeaturedCount(int Count);
}
