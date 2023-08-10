namespace Kiss.Bff.InterneTaak
{
    public static class InterneTaakExtensions
    {
        public static IServiceCollection AddInterneTaakProxy(this IServiceCollection services, InterneTaakSettings settings) => services.AddSingleton(settings);
    }

    public record InterneTaakSettings(string Destination, string Token, string ObjectTypeUrl);
}
