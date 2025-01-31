using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers
{
    internal static class Locators
    {
 
         public static ILocator Personen_LastNameInput(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "Achternaam" });

         public static ILocator Personen_BirthDateInput(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "Geboortedatum" });

         public static ILocator PersonenFirst_SearchButton(this IPage page) =>
            page.Locator("form").Filter(new () { HasText = "Achternaam Geboortedatum" }).GetByRole(AriaRole.Button);
        public static ILocator PersonenSecond_SearchButton(this IPage page) =>
          page.Locator("form").Filter(new() { HasText = "Postcode Huisnummer" }).GetByRole(AriaRole.Button);
       
        public static ILocator PersonenThird_SearchButton(this IPage page) =>
            page.Locator("form").Filter(new() { HasText = "Bsn Zoeken" }).GetByRole(AriaRole.Button);
        public static ILocator Personen_PostCodeInput(this IPage page) =>
           page.GetByRole(AriaRole.Textbox, new() { Name = "postcode" });
        public static ILocator Personen_HuisnummerInput(this IPage page) =>
          page.GetByRole(AriaRole.Textbox, new() { Name = "huisnummer" });

        public static async Task<ILocator> SearchAddressByPostalAndHuisNummer(this IPage page, string postcode, string huisNummer)
        {
            var tableLocator = page.GetByRole(AriaRole.Table); // Locate the table
            await page.WaitForSelectorAsync("tbody tr"); // Ensure table rows are loaded

            // Construct the locator
            return tableLocator.Locator($"tbody tr:has(td:nth-child(4):has-text(\"{postcode.Trim()}\"))" +
                                         $":has(td:nth-child(3):has-text(\"{huisNummer.Trim()}\"))");
        }
    }


}

