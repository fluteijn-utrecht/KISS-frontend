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
          
     }

    
}

