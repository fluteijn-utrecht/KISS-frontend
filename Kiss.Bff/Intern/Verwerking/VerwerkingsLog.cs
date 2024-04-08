namespace Kiss.Bff.Beheer.Verwerking
{
    public class VerwerkingsLog
    {
        public Guid Id { get; private set; } = default;
        public string? UserId { get; set; }
        public DateTimeOffset InsertedAt { get; private set; }
        public string ApiEndpoint { get; set; } = default!;
        public string Method { get; set; } = default!;
    }
}
