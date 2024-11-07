using System.Text.Json;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;
using Kiss.Bff.Beheer.Links.Data.Entities;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Seed.Features
{
    [ApiController]
    [Route("api/seed")]
    [Authorize(Policy = Policies.RedactiePolicy)]
    public class SeedController : ControllerBase
    {
        private readonly BeheerDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeedController(BeheerDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private async Task<bool> AnyRecordsExistAsync()
        {
            return await _context.Berichten.AnyAsync() ||
                await _context.Skills.Where(x => !x.IsDeleted).AnyAsync() || // Check ...
                await _context.Links.AnyAsync() ||
                await _context.Gespreksresultaten.AnyAsync();
        }

        private static List<T> ReadListFromJsonFile<T>(IWebHostEnvironment environment, string file)
        {
            var path = Path.Combine(environment.ContentRootPath, "json/" + file);
            var json = System.IO.File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        [HttpPost("start")]
        public async Task<IActionResult> Seed()
        {
            try
            {
                if (await AnyRecordsExistAsync())
                {
                    return Conflict("Database is already populated. Seeding skipped.");
                }

                // berichten
                var berichten = ReadListFromJsonFile<Bericht>(_environment, "berichten.json");

                foreach (var bericht in berichten)
                {
                    bericht.DateCreated = DateTimeOffset.UtcNow;
                    bericht.PublicatieDatum = DateTimeOffset.UtcNow;
                    bericht.PublicatieEinddatum = DateTimeOffset.UtcNow.AddYears(1);
                }

                await _context.Berichten.AddRangeAsync(berichten);

                // skills
                var skills = ReadListFromJsonFile<Skill>(_environment, "skills.json");

                foreach (var skill in skills)
                {
                    skill.DateCreated = DateTimeOffset.UtcNow;
                }

                await _context.Skills.AddRangeAsync(skills);

                // links
                var links = ReadListFromJsonFile<Link>(_environment, "links.json");

                foreach (var link in links)
                {
                    link.DateCreated = DateTimeOffset.UtcNow;
                }

                await _context.Links.AddRangeAsync(links);

                // gespreksresultaten
                var gespreksresultaten = ReadListFromJsonFile<Gespreksresultaat>(_environment, "gespreksresultaten.json");

                foreach (var gespreksresultaat in gespreksresultaten)
                {
                    gespreksresultaat.DateCreated = DateTimeOffset.UtcNow;
                }

                await _context.Gespreksresultaten.AddRangeAsync(gespreksresultaten);

                // save all changes
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
            if (await AnyRecordsExistAsync())
            {
                return Conflict("Database is already populated.");
            }

            return Ok("Database is not populated.");
        }
    }
}
