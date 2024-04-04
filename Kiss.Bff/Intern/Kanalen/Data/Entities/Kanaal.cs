using System.ComponentModel.DataAnnotations;

namespace Kiss.Bff.Intern.Kanalen.Data.Entities
{
    public class Kanaal
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string Naam { get; set; } = string.Empty;       
    }
}
