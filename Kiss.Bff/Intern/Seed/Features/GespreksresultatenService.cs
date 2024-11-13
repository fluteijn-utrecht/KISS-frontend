using Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities;

namespace Kiss.Bff.Intern.Seed.Features
{
    public class GespreksresultatenService
    {
        public List<Gespreksresultaat> GenerateGespreksresultaten()
        {
            return new List<Gespreksresultaat>
            {
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Afgehandeld, klant belt terug"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Afgehandeld na ruggespraak"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Doorverbonden naar afdeling"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Doorverbonden naar collega"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Klant neemt opnieuw contact op"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Verbinding verbroken"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Verwezen naar ander instantie"
                },
                new()
                {
                    DateCreated = DateTimeOffset.UtcNow,
                    Definitie = "Zelfstandig afgehandeld"
                }
            };
        }
    }
}
