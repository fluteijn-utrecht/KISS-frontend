using Kiss.Bff.NieuwsEnWerkinstructies.Data;
using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly NieuwsEnWerkinstructiesDbContext _context;

        public SkillsController(NieuwsEnWerkinstructiesDbContext context)
        {
            _context = context;
        }

        // GET: api/Skills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
        {
            if (_context.Skills == null)
            {
                return NotFound();
            }
            return await _context.Skills.ToListAsync();
        }

        // GET: api/Skills/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> GetSkill(int id)
        {
            if (_context.Skills == null)
            {
                return NotFound();
            }
            var skill = await _context.Skills.FindAsync(id);

            if (skill == null)
            {
                return NotFound();
            }

            return skill;
        }

        // PUT: api/Skills/5        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkill(int id, SkillPutModel skill)
        {
            var current = _context.Skills.FirstOrDefault(x => x.Id == id);

            if (current == null)
            {
                return NotFound();
            }

            current.Naam = skill.Naam;

            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // POST: api/Skills        
        [HttpPost]
        public async Task<ActionResult<Skill>> PostSkill(SkillPostModel skill)
        {
            if (_context.Skills == null)
            {
                return Problem("Entity set 'CmsDbContext.Skills'  is null.");
            }

            var newSkill = new Skill { Naam = skill.Naam };

            _context.Skills.Add(newSkill);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSkill", new { id = newSkill.Id }, newSkill);
        }

        // DELETE: api/Skills/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {

            throw new NotImplementedException();

            if (_context.Skills == null)
            {
                return NotFound();
            }
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }


    public class SkillPutModel
    {
        [Required]
        public string Naam { get; set; } = default!;
    }

    public class SkillPostModel
    {
        [Required]
        public string Naam { get; set; } = default!;
    }
}
