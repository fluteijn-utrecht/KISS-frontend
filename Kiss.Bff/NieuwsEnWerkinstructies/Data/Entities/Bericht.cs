namespace Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities
{
    public class Bericht
    {
        public int Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }


        public DateTimeOffset? PublicatieDatum { get; set; }
        public DateTimeOffset? PublicatieEinddatum { get; set; }
        public string Titel { get; set; } = string.Empty;
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }
        public string Type { get; set; } = string.Empty;


        public List<Skill> Skills { get; } = new ();
    }
}
