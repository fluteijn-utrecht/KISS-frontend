namespace Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public string Naam { get; set; } = string.Empty;
    }
}
