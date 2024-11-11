using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    public class ContactmomentDetailsBronModel
    {
        [Required]
        public string Soort { get; set; } = "";
        [Required]
        public string Titel { get; set; } = "";
        [Required]
        public string Url { get; set; } = "";
    }
}
