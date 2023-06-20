using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Beheer.Management
{


    [ApiController]
    public class GetContactmomentManagementLog : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public GetContactmomentManagementLog(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/management/contactmomenten")]
        public IActionResult Get()
        {

            var contactmomenten =  _db.ContactmomentManagementLogs
                .OrderByDescending(x => x.Startdatum)
                .Select(x => new { x.EmailadresKcm })
                .Take(10000)
                .AsAsyncEnumerable();

            return Ok(contactmomenten);
        }
    }
}
