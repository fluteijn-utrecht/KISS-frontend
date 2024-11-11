using Kiss.Bff.Beheer.Data;
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
        private readonly BerichtenService _berichtenService;
        private readonly SkillsService _skillsService;
        private readonly LinksService _linksService;
        private readonly GespreksresultatenService _gespreksresultatenService;

        public SeedController(
            BeheerDbContext context,
            BerichtenService berichtenService,
            SkillsService skillsService,
            LinksService linksService,
            GespreksresultatenService gespreksresultatenService
        )
        {
            _context = context;
            _berichtenService = berichtenService;
            _skillsService = skillsService;
            _linksService = linksService;
            _gespreksresultatenService = gespreksresultatenService;
        }

        private async Task<bool> AnyRecordsExistAsync()
        {
            return await _context.Berichten.AnyAsync() ||
                await _context.Skills.AnyAsync() ||
                await _context.Links.AnyAsync() ||
                await _context.Gespreksresultaten.AnyAsync();
        }

        [HttpPost("start")]
        public async Task<IActionResult> SeedStart()
        {
            try
            {
                if (await AnyRecordsExistAsync())
                {
                    return Conflict("Database is already populated. Seeding skipped.");
                }

                // berichten
                var berichten = _berichtenService.GenerateBerichten();
                await _context.Berichten.AddRangeAsync(berichten);

                // skills
                var skills = _skillsService.GenerateSkills();
                await _context.Skills.AddRangeAsync(skills);

                // links
                var links = _linksService.GenerateLinks();
                await _context.Links.AddRangeAsync(links);

                // gespreksresultaten
                var gespreksresultaten = _gespreksresultatenService.GenerateGespreksresultaten();
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
