using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Kiss.Bff.Intern.Gespreksresultaten.Features;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Seed.Features
{
    [ApiController]
    [Route("api/seed")]
    [Authorize(Policy = Policies.RedactiePolicy)]
    public class DataSeedController : ControllerBase
    {
        private readonly BeheerDbContext _context;

        public DataSeedController(BeheerDbContext context)
        {
            _context = context;
        }

        private async Task<bool> AnyRecordsExistAsync()
        {
            return await _context.Berichten.AnyAsync() ||
                await _context.Skills.Where(x => !x.IsDeleted).AnyAsync() ||
                await _context.Links.AnyAsync() ||
                await _context.Gespreksresultaten.AnyAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            try
            {
                if (await AnyRecordsExistAsync())
                {
                    return BadRequest("Data already exists in the database. Seeding skipped.");
                }

                // berichten
                var berichten = new List<Bericht>
                {
                    new()
                    {
                        PublicatieDatum =  DateTimeOffset.UtcNow,
                        PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1),
                        DateCreated = DateTimeOffset.UtcNow,
                        Titel = "Nieuws",
                        Inhoud = "<p>Nieuws</p>",
                        IsBelangrijk = true,
                        Type = "Nieuws"
                    }
                };

                await _context.Berichten.AddRangeAsync(berichten);

                // skills
                var skills = new List<Skill>
                {
                    new()
                    {
                        DateCreated = DateTimeOffset.UtcNow,
                        Naam = "Skill één"
                    }
                };

                await _context.Skills.AddRangeAsync(skills);

                // links
                var links = new List<Link>
                {
                    new()
                    {
                        DateCreated = DateTimeOffset.UtcNow,
                        Titel = "Link",
                        Url = "https://www.icatt.nl",
                        Categorie = "Links"
                    }
                };

                await _context.SaveChangesAsync();

                return Ok("Data seeded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> SeedCheck()
        {
            try
            {
                if (await AnyRecordsExistAsync())
                {
                    return BadRequest("Data already exists in the database.");
                }

                return Ok("No data in the database.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
