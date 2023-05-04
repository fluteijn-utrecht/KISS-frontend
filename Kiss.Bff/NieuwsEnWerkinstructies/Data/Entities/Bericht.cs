namespace Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities
{
    public class Bericht
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }


        public DateTime? PublicatieDatum { get; set; }
        public string Titel { get; set; } = string.Empty;
        public string Inhoud { get; set; } = string.Empty;
        public bool IsBelangrijk { get; set; }

        public List<Skill> Skills { get; set; } = new List<Skill>();
    }
}
