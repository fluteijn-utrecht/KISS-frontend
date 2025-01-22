using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers
{
    internal static class Locators
    {
        // Get the Last Name input field
        public static ILocator GetLastNameInputAsync(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "achternaam" });
        }

        // Get the Date of Birth input field
        public static ILocator GetDOBInputAsync(this IPage page)
        {
            return  page.GetByRole(AriaRole.Textbox, new() { Name = "geboortedatum" });
        }

        // Get the BSN input field
        public static ILocator GetBSNInputAsync(this IPage page)
        {
            return  page.GetByRole(AriaRole.Textbox, new() { Name = "bsn" });
        }

        // Get the Postcode input field
        public static ILocator GetPostcodeInputAsync(this IPage page)
        {
            return  page.GetByRole(AriaRole.Textbox, new() { Name = "postcode" });
        }

        // Get the Huisnummer input field
        public static ILocator GetHuisnummerInputAsync(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "huisnummer" });
        }

        // Get the Search button
        public static ILocator GetSearchButtonAsync(this IPage page)
        {
            return  page.GetByRole(AriaRole.Button, new() { Name = "searchButton" });
        }

        // Get the error message element
        public static ILocator GetErrorMessageAsync(this IPage page)
        {
            return  page.GetByRole(AriaRole.Alert);
        }

        // Get all the results list items
        public static  ILocator GetResultsListAsync(this IPage page)
        {
            return page.GetByRole(AriaRole.Listitem);
        }

    }
      

    }

