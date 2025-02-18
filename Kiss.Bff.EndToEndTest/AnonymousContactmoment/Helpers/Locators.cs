using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers
{
    public static class Locators
    {
        // During development, it is essential to include the ARIA role to ensure the element is located accurately, rather than relying on the class name.
        public static ILocator GetGlobalSearch(this IPage page)
        {
            return page.Locator(".contactmomentLoopt").GetByRole(AriaRole.Combobox);
        }

        public static ILocator GetGlobalSearchResults(this IPage page)
        {
            return page.Locator(".search-results.isExpanded").GetByRole(AriaRole.Navigation).First.GetByRole(AriaRole.Link);
        }

        public static ILocator GetSmoelenboekCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "Smoelenboek" });
        }

        public static ILocator GetVACCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "VAC" });
        }

        public static ILocator GetKennisbankCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "Kennisbank" });
        }
    }
   }
