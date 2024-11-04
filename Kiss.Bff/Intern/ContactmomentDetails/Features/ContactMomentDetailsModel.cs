namespace Kiss.Bff.Intern.ContactmomentDetails.Features
{
    public record ContactmomentDetailsModel(string Id, DateTimeOffset Einddatum, string? EmailadresKcm, string? Gespreksresultaat, string? SpecifiekeVraag, DateTimeOffset Startdatum, string? VerantwoordelijkeAfdeling, string? Vraag, IEnumerable<BronModel> Bronnen);
    public record BronModel(string Soort, string Titel, string Url);

}
