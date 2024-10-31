using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
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
            model.EmailadresKcm = User.GetEmail();
            var existingModel = await _db.ContactMomentDetails
                .Include(c => c.Bronnen) 
                .FirstOrDefaultAsync(c => c.Id == model.Id, cancellationToken);

            if (existingModel == null)
            {
                await _db.AddAsync(model, cancellationToken);
            }
            else
            {
                _db.Entry(existingModel).CurrentValues.SetValues(model);

                var existingBronIds = existingModel.Bronnen.Select(b => b.Id).ToList();
                var incomingBronIds = model.Bronnen.Select(b => b.Id).ToList();

                var bronnenToDelete = existingModel.Bronnen
                    .Where(b => !incomingBronIds.Contains(b.Id))
                    .ToList();

                _db.Bronnen.RemoveRange(bronnenToDelete);

                foreach (var incomingBron in model.Bronnen)
                {
                    var existingBron = existingModel.Bronnen
                        .FirstOrDefault(b => b.Id == incomingBron.Id);

                    if (existingBron != null)
                    {
                        _db.Entry(existingBron).CurrentValues.SetValues(incomingBron);
                    }
                    else
                    {
                        existingModel.Bronnen.Add(incomingBron);
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}
