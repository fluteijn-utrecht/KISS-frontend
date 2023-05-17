namespace Kiss.Bff.Beheer.Gespreksresultaten.Data.Entities
{
    public class Gespreksresultaat
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

        public string Definitie { get; set; } = string.Empty;
    }
}
