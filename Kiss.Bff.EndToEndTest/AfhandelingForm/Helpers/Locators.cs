using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers
{
    internal static class Locators
    {

        public static ILocator GetNieuwContactmomentButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });
        }
        public static ILocator GetPersonenNotitieblokTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Tabpanel, new() { Name = "Notitieblok" }).GetByRole(AriaRole.Textbox);
        }
         
        public static ILocator GetPersonenAfrondenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Afronden" });
        }
         
        public static ILocator GetAfhandelingNotitieTextBox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Notitie" });
        }
    }
}
