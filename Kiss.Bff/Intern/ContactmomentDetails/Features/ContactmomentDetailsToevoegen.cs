using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Intern.ContactmomentDetails.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    [ApiController]
    public class ContactmomentDetailsToevoegen : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public ContactmomentDetailsToevoegen(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpPut("/api/contactmomentdetails")]
        public async Task<IActionResult> Post(ContactmomentDetailsModel model, CancellationToken cancellationToken)
        {
            var entity = await _db.ContactMomentDetails.Include(x => x.Bronnen).FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken: cancellationToken);

            if(entity == null)
            {
                entity = new Data.Entities.ContactmomentDetails
                {
                    Id = model.Id,
                    Einddatum = model.Einddatum,
                    EmailadresKcm = User.GetEmail(),
                    Gespreksresultaat = model.Gespreksresultaat,
                    SpecifiekeVraag = model.SpecifiekeVraag,
                    Startdatum = model.Startdatum,
                    VerantwoordelijkeAfdeling = model.VerantwoordelijkeAfdeling,
                    Vraag = model.Vraag,
                    Bronnen = new List<ContactmomentDetailsBron>()
                };
                await _db.AddAsync(entity, cancellationToken);
            }

            entity.Bronnen.Clear();

            foreach (var bron in model.Bronnen.Select(b => new ContactmomentDetailsBron { Soort = b.Soort, Titel = b.Titel, Url = b.Url }))
            {
                entity.Bronnen.Add(bron);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}
