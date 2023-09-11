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
            //denaamvandeclaim moet uit de env.local komen
            //var userIdClaim = User.Claims[""denaamvandeclaim"]
            //model.ietsanders = useridclaim;
            var userIdClaimName = Environment.GetEnvironmentVariable("MEDEWERKER_INDENTIFICATIE_CLAIM");
            var userIdClaimValue = User.Claims.FirstOrDefault(c => c.Type == userIdClaimName)?.Value;

            //foreach (var claim in User.Claims)
            //{
            //    Console.WriteLine($"{claim.Type}: {claim.Value}");
            //}

            if (userIdClaimValue != null && userIdClaimValue.Length <= 24)
            {
                model.UserClaimIndentifier = userIdClaimValue;
            }
            else
            {
                model.EmailadresKcm = User.GetEmail();
            }
            
            
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
