using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Kiss.Bff.NieuwsEnWerkinstructies.Data;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.NieuwsEnWerkinstructies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BerichtenController : ControllerBase
    {
        private readonly NieuwsEnWerkinstructiesDbContext _context;

        public BerichtenController(NieuwsEnWerkinstructiesDbContext context)
        {
            _context = context;
        }

        // GET: api/Berichten
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bericht>>> GetBerichten()
        {
            if (_context.Berichten == null)
            {
                return NotFound();
            }
            return await _context.Berichten.ToListAsync();
        }

        // GET: api/Berichten/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BerichtViewModel>> GetBericht(int id)
        {
            if (_context.Berichten == null)
            {
                return NotFound();
            }

            var bericht = await _context.Berichten
                .Include(x => x.Skills.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bericht == null)
            {
                return NotFound();
            }

            return new BerichtViewModel
            {
                Id = bericht.Id,
                Inhoud = bericht.Inhoud,
                IsBelangrijk = bericht.IsBelangrijk,
                PublicatieDatum = bericht.PublicatieDatum,
                Titel = bericht.Titel,
                Skills = bericht.Skills
                    .Select(skill => new BerichtSkillViewModel { Id = skill.Id, Naam = skill.Naam })
                    .ToList()            
            };
        }

        // PUT: api/Berichten/5      
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBericht(int id, BerichtPutModel bericht)
        {
            var current = _context.Berichten.Include(x => x.Skills).FirstOrDefault(x => x.Id == id);

            if (current == null)
            {
                return NotFound();
            }

            current.Titel = bericht.Titel;
            current.Inhoud = bericht.Inhoud;
            current.PublicatieDatum = bericht.PublicatieDatum;
            current.IsBelangrijk = bericht.IsBelangrijk;
            current.DateUpdated = DateTimeOffset.Now;

            UpdateSkills(bericht.Skills, current);


            await _context.SaveChangesAsync();

            return NoContent();
        }



        // POST: api/Berichten
        [HttpPost]
        public async Task<ActionResult<Bericht>> PostBericht(BerichtPostModel bericht)
        {
            if (_context.Berichten == null)
            {
                return Problem("Entity set 'CmsDbContext.Berichten'  is null.");
            }

            var newBericht = new Bericht
            {
                Titel = bericht.Titel,
                Inhoud = bericht.Inhoud,
                PublicatieDatum = bericht.PublicatieDatum,
                IsBelangrijk = bericht.IsBelangrijk,
                DateCreated = DateTimeOffset.Now,

            };

            UpdateSkills(bericht.Skills, newBericht);

            _context.Berichten.Add(newBericht);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBericht", new { id = newBericht.Id }, newBericht);

        }

        // DELETE: api/Berichten/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBericht(int id)
        {
            if (_context.Berichten == null)
            {
                return NotFound();
            }
            var bericht = await _context.Berichten.FindAsync(id);
            if (bericht == null)
            {
                return NotFound();
            }

            _context.Berichten.Remove(bericht);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private void UpdateSkills(List<int>? selectedSkills, Bericht? current)
        {
            if (current == null)
            {
                return;
            }

            //reset 
            current.Skills.Clear();

            //add selected skills
            if (selectedSkills != null)
            {
                //available skills
                var skills = _context.Skills.ToList();

                foreach (var skillId in selectedSkills)
                {
                    var dbSkill = skills.FirstOrDefault(x => x.Id == skillId);
                    if (dbSkill != null)
                    {
                        current.Skills.Add(dbSkill);
                    }
                }
            }
        }

    }


    public class BerichtPutModel
    {
        [Required]
        public string Titel { get; set; } = string.Empty;
        [Required]
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }
        public List<int>? Skills { get; set; }
        public DateTime? PublicatieDatum { get; set; }
    }

    public class BerichtPostModel
    {
        [Required]
        public string Titel { get; set; } = string.Empty;
        [Required]
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }
        public List<int>? Skills { get; set; }
        public DateTimeOffset? PublicatieDatum { get; set; }
    }

    public class BerichtViewModel
    {
        public int Id { get; set; }

        public DateTimeOffset? PublicatieDatum { get; set; }
        public string Titel { get; set; } = string.Empty;
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }

        public List<BerichtSkillViewModel> Skills { get; set; } = new();
    }


    public class BerichtSkillViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; } = string.Empty;
    }

}

