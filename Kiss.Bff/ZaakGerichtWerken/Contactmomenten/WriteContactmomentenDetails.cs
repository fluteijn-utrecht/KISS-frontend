using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    [ApiController]
    public class WriteContactmomentenDetails : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public WriteContactmomentenDetails(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpPut("/api/contactmomentdetails")]
        public async Task<IActionResult> Post(ContactmomentDetails model, CancellationToken cancellationToken)
        {
            model.EmailadresKcm = User.GetEmail();
            await _db.AddAsync(model, cancellationToken);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                _db.Entry(model).State = EntityState.Modified;
                await _db.SaveChangesAsync(cancellationToken);
            }
            return Ok();
        }
    }
}
