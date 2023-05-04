namespace Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public string Naam { get; set; } = string.Empty;
    }
}
