using Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities;

namespace Kiss.Bff.Intern.Seed.Features
{
    public class SkillsService
    {
        public List<Skill> GenerateSkills()
        {
            return new List<Skill>
            {
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam = "afval"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "algemeen"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "belastingen"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "burgerzaken"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "KCC"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "subsidies"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "vergunningen"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Naam =  "werk en inkomen"
                }
            };
        }
    }
}
