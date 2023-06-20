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
