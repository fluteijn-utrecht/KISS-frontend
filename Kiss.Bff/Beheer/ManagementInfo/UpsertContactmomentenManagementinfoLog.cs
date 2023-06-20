using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Beheer.Managementinfo
{
    [ApiController]
    public class UpsertContactmomentenManagementinfoLog: ControllerBase
    {
        private readonly BeheerDbContext _db;

        public UpsertContactmomentenManagementinfoLog(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpPut("/api/managementInfo/contactmomenten")]
        public async Task<IActionResult> Post(ContactmomentManagementinfoLog model, CancellationToken cancellationToken)
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
