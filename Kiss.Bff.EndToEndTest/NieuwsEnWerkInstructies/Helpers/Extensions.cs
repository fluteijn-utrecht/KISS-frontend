namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class Extensions
    {
        public static ILocator GetNieuwsSection(this IPage page) => page.Locator("section").Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = "Nieuws" }) });
        public static ILocator GetWerkinstructiesSection(this IPage page) => page.Locator("section").Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = "Werkinstructies" }) });
        public static ILocator GetBerichtOnHomePage(this IPage page, Bericht bericht) => page.GetByRole(AriaRole.Article).Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = bericht.Titel }) });

        public static async Task NavigateToNieuwsWerkinstructiesBeheer(this IPage page)
        {
            var beheerlink = page.GetByRole(AriaRole.Link, new() { Name = "Beheer" });
            var berichtenlink = page.GetByRole(AriaRole.Link, new() { Name = "Nieuws en werkinstructies" });

            await beheerlink.Or(berichtenlink).First.WaitForAsync();

            if (await beheerlink.IsVisibleAsync())
            {
                await beheerlink.ClickAsync();
            }

            await beheerlink.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            if (await berichtenlink.GetAttributeAsync("aria-current") != "page")
            {
                await berichtenlink.ClickAsync();
                await page.GetByRole(AriaRole.Heading, new() { Name = "Berichten" }).WaitForAsync();
            }
        }

        public static async Task NavigateToSkillsBeheer(this IPage page)
        {
            var beheerlink = page.GetByRole(AriaRole.Link, new() { Name = "Beheer" });
            var skillslink = page.GetByRole(AriaRole.Link, new() { Name = "Skills" });
            await beheerlink.Or(skillslink).First.WaitForAsync();

            if (await beheerlink.IsVisibleAsync())
            {
                await beheerlink.ClickAsync();
            }

            await beheerlink.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            if (await skillslink.GetAttributeAsync("aria-current") != "page")
            {
                await skillslink.ClickAsync();
                await page.GetByRole(AriaRole.Heading, new() { Name = "Skills" }).WaitForAsync();
            }
        }

        public static ILocator GetSummaryElement(this IPage page) => page.Locator("summary");
        public static ILocator GetSkillsSummaryElement(this IPage page) => page.GetSummaryElement().Filter(new () { HasText = "Filter op categorie" });
        public static ILocator GetSkillsFieldset(this IPage page) => page.GetByRole(AriaRole.Group, new() { Name = "Filter op categorie" });
    }
}
