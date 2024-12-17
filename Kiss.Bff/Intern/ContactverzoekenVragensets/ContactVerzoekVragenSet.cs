using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.Intern.ContactverzoekenVragensets
{
    public class ContactVerzoekVragenSet
    {
        [Required]
        public int Id { get; set; } = default!;
        [Required]
        public string Titel { get; set; } = default!;
        public string? JsonVragen { get; set; }
        public string OrganisatorischeEenheidId { get; set; } = default!;
        public string OrganisatorischeEenheidNaam { get; set; } = default!;

        public string OrganisatorischeEenheidSoort { get; set; } = default!;
    }
}
