using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Beheer.Management
{
    [ApiController]
    public class UpsertContactmomentManagementLog: ControllerBase
    {
        private readonly BeheerDbContext _db;

        public UpsertContactmomentManagementLog(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpPut("/api/management/contactmoment")]
        public async Task<IActionResult> Post(ContactmomentManagementLog model, CancellationToken cancellationToken)
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
