namespace Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers
{
    internal static class Navigation
    {
       
        public static async Task NavigateToContactMomentAsync(this IPage page)
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }).ClickAsync();

        }
    }
}
