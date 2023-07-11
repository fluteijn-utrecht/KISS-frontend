using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Beheer.Faq
{
    [ApiController]
    [Route("api/faq")]
    public class GetFaq: ControllerBase
    {
        private const int AmountOfContactmomenten = 500;
        private const int AmountOfQuestions = 10;

        private readonly BeheerDbContext _db;

        public GetFaq(BeheerDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var lastContactmomenten = _db.ContactmomentManagementLogs
                .OrderByDescending(x => x.Einddatum)
                .Take(AmountOfContactmomenten);

            var groupedByQuestion = lastContactmomenten
                .Where(x => !string.IsNullOrWhiteSpace(x.PrimaireVraagWeergave) || !string.IsNullOrWhiteSpace(x.AfwijkendOnderwerp))
                .GroupBy(x => string.IsNullOrWhiteSpace(x.PrimaireVraagWeergave) 
                    ? x.AfwijkendOnderwerp 
                    : x.PrimaireVraagWeergave);

            var topQuestions = groupedByQuestion
                .OrderByDescending(x => x.Count())
                .Take(AmountOfQuestions);

            var query = topQuestions
                .Select(x => x.Key)
                .AsAsyncEnumerable();

            return Ok(query);
        }
    }
}
