using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kiss.Bff.ZaakGerichtWerken.Contactmomenten
{
    public class BronModel
    {
        public int Id { get; set; }

        public string ContactmomentDetailsId { get; set; } = default!;

        public string Soort { get; set; } = string.Empty;

        public string Titel { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public ContactmomentDetailsModel ContactmomentDetails { get; set; } = null!;
    }
}
