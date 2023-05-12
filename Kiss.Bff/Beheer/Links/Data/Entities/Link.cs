namespace Kiss.Bff.Beheer.Links.Data.Entities
{
    public class Link
    {
        public int Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

        public string Titel { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
    }
}
