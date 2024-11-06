using System.Net.Http.Json;
using System.Text.Json;
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
                await _context.Skills.Where(x => !x.IsDeleted).AnyAsync() ||
                await _context.Links.AnyAsync() ||
                await _context.Gespreksresultaten.AnyAsync();
        }

        private static List<T> ReadListFromJsonFile<T>(IWebHostEnvironment environment, string file)
        {
            var path = Path.Combine(environment.ContentRootPath, "json/" + file);
            var json = System.IO.File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            try
            {
                if (await AnyRecordsExistAsync())
                {
                    return Conflict("Database is already populated. Seeding skipped.");
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
                //var skills = new List<Skill>
                //{
                //    new()
                //    {
                //        DateCreated = DateTimeOffset.UtcNow,
                //        Naam = "Skill één"
                //    }
                //};

                var skills = ReadListFromJsonFile<Skill>(_environment, "skills.json");

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

                await _context.Links.AddRangeAsync(links);

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
                    return Conflict("Database is already populated.");
                }

                return Ok("Database is not populated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
