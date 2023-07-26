using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Beheer.Managementinfo
{


    [ApiController]
    public class GetContactmomentenManagementinfoLog : ControllerBase
    {
        private readonly BeheerDbContext _db;

        public GetContactmomentenManagementinfoLog(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet("/api/managementinfo/contactmoment")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public async Task <IActionResult> Get([FromQuery] string id, CancellationToken token)
        {
            var contactmoment = await _db.ContactmomentManagementLogs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (contactmoment == null)
            {
                return NotFound();
            }

            return Ok(contactmoment);
        }

        [HttpGet("/api/managementinfo/contactmomenten")]
        [Authorize(Policy = Policies.RedactiePolicy)]
        public IActionResult Get()
        {
            var contactmomenten =  _db.ContactmomentManagementLogs
                .OrderByDescending(x => x.Startdatum)              
                .Take(10000)
                .AsAsyncEnumerable();
            
            return Ok(contactmomenten);
        }
    }
}
