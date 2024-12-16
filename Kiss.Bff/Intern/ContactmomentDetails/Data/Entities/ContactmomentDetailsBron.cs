namespace Kiss.Bff.Intern.ContactmomentDetails.Data.Entities
{
    public class ContactmomentDetailsBron
    {
        public int Id { get; set; }

        public string ContactmomentDetailsId { get; set; } = default!;

        public string Soort { get; set; } = string.Empty;

        public string Titel { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public ContactmomentDetails ContactmomentDetails { get; set; } = null!;
    }
}
