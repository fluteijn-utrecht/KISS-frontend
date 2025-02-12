using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers
{
    public static class Locators
    {
        public static ILocator GetNieuwContactmomentButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });
        }

        public static ILocator GetContactmomentSearch(this IPage page)
        {
            return page.Locator(".contactmomentLoopt").GetByRole(AriaRole.Combobox);
        }
    

        public static ILocator GetContactmomentSearchResults(this IPage page)
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
