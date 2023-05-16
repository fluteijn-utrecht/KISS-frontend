namespace Kiss.Bff.NieuwsEnWerkinstructies.Data.Entities
{
    public class BerichtGelezen
    {
        public int BerichtId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset GelezenOp { get; set; }
    }
}
