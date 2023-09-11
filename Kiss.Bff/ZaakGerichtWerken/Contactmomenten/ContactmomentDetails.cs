using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    public class ContactmomentDetails
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
        public string? UserClaimIndentifier { get; set; } 
    }
}
