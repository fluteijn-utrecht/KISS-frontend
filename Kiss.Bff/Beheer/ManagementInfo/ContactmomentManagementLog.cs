using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.Beheer.Managementinfo
{
    public class ContactmomentManagementinfoLog
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        public DateTimeOffset Startdatum { get; set; }
        [Required]
        public DateTimeOffset Einddatum { get; set; }
        public string? Resultaat { get; set; }
        public string? PrimaireVraagWeergave { get; set; }
        public string? AfwijkendOnderwerp { get; set; }
        public string? EmailadresKcm { get; set; }
    }
}
