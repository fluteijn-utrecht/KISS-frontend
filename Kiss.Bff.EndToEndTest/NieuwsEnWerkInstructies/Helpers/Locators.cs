namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class Locators
    {
        public static ILocator GetNieuwsSection(this IPage page) => page.Locator("section").Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = "Nieuws" }) });
        public static ILocator GetWerkinstructiesSection(this IPage page) => page.Locator("section").Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = "Werkinstructies" }) });
        public static ILocator GetBerichtOnHomePage(this IPage page, Bericht bericht) => page.GetByRole(AriaRole.Article).Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = bericht.Title }) });
        public static ILocator GetSummaryElement(this IPage page) => page.Locator("summary");
        public static ILocator GetSkillsSummaryElement(this IPage page) => page.GetSummaryElement().Filter(new() { HasText = "Filter op categorie" });
        public static ILocator GetSkillsFieldset(this IPage page) => page.GetByRole(AriaRole.Group, new() { Name = "Filter op categorie" });
        public static ILocator GetElementById(this IPage page, string id) => page.Locator($"#{id}");
        public static ILocator GetWerkberichtTypeSelector(this IPage page) => page.GetElementById("werkbericht-type-input");
        public static ILocator GetNieuwsAndWerkinstructiesSearch(this IPage page) => page.GetElementById("search-input");
        public static ILocator GetSearchResult(this IPage page) => page.Locator("section").Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = "Zoekresultaten" }) });

    }
}
