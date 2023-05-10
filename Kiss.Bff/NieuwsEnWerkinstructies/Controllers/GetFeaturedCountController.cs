﻿using Kiss.Bff.NieuwsEnWerkinstructies.Data;
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
        public async Task<ActionResult<FeaturedCount>> GetCount(CancellationToken token)
        {
            var userId = User.GetId();
            
            var count = await _context.Berichten
                .Select(x=> new 
                {
                    x.IsBelangrijk, 
                    x.Gelezen, 
                    Datum = x.DateUpdated > x.PublicatieDatum 
                        ? x.DateUpdated 
                        : x.PublicatieDatum 
                })
                .Where(x => x.IsBelangrijk && !x.Gelezen.Any(g => g.UserId == userId && g.GelezenOp >= x.Datum))
                .CountAsync(token);

            return new FeaturedCount(count);
        }
    }

    public record struct FeaturedCount(int Count);
}