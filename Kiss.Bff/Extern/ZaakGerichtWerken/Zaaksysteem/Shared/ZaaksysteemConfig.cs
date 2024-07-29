namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared
{
    public record ZaaksysteemConfig(string BaseUrl, string ClientId, string Secret, string? DeeplinkBaseUrl, string? DeeplinkProperty);
}
