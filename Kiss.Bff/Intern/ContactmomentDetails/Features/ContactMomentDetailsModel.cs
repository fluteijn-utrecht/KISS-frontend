using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    public class ContactmomentDetailsModel
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        public DateTimeOffset Startdatum { get; set; }
        [Required]
        public DateTimeOffset Einddatum { get; set; }
        public string? Gespreksresultaat { get; set; }
        public string? Vraag { get; set; }
        public string? SpecifiekeVraag { get; set; }
        public string? EmailadresKcm { get; set; }
        public string? VerantwoordelijkeAfdeling { get; set; }
        
        public IEnumerable<ContactmomentDetailsBronModel> Bronnen { get; set; } = Enumerable.Empty<ContactmomentDetailsBronModel>();
    }

    internal static class ContactmomentDetailsModelExtensions
    {
        public static IQueryable<ContactmomentDetailsModel> ToModel(this IQueryable<Data.Entities.ContactmomentDetails> queryable) => queryable.Select(x => new ContactmomentDetailsModel
        {
            Id = x.Id,
            Einddatum = x.Einddatum,
            EmailadresKcm = x.EmailadresKcm,
            Gespreksresultaat = x.Gespreksresultaat,
            SpecifiekeVraag = x.SpecifiekeVraag,
            Startdatum = x.Startdatum,
            VerantwoordelijkeAfdeling = x.VerantwoordelijkeAfdeling,
            Vraag = x.Vraag,
            Bronnen = x.Bronnen.Select(b => new ContactmomentDetailsBronModel
            {
                Soort = b.Soort,
                Titel = b.Titel,
                Url = b.Url
            })
        });
    }
}
