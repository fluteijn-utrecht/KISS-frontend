using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentZaak.Helpers
{
   
        public static class Locators
        {
            public static ILocator GetNieuwContactmomentButton(this IPage page)
            {
                return page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });
            }

            public static ILocator GetContactmomentSearch(this IPage page)
            {
                return page.GetByRole(AriaRole.Combobox, new() { Name = "Zoekterm" });
            }

            public static ILocator GetContactmomentSearchResults(this IPage page)
            {
                return page.Locator(".search-results.isExpanded ul li");
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

            public static ILocator GetDeventerCheckbox(this IPage page)
            {
                return page.GetByRole(AriaRole.Checkbox, new() { Name = "Deventer.nl" });
            }

            public static ILocator GetZakenMenu(this IPage page)
            {
                return page.GetByRole(AriaRole.Link, new() { Name = "Zaken" });
            }

            public static ILocator GetZaakSearchField(this IPage page)
            {
                return page.GetByTitle("ZAAK-1");
            }

            public static ILocator GetZaakSearchButton(this IPage page)
            {
                return page.GetByTitle("Zoeken");
            }

            public static ILocator GetZaakScreen(this IPage page, string zaakId)
            {
                return page.GetByRole(AriaRole.Heading, new() { Name = $"Zaak {zaakId}" });
            }

            public static ILocator GetPersonenAfrondenButton(this IPage page)
            {
                return page.GetByRole(AriaRole.Button, new() { Name = "Afronden" });
            }

            public static ILocator GetAfhandelingForm(this IPage page)
            {
            return page.Locator("form.afhandeling");
        }

            public static ILocator GetGerelateerdeZaakHeading(this IPage page)
            {
            var vraagHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Vraag 1" });
           
            return vraagHeading.Locator("..").GetByRole(AriaRole.Heading, new() { Name = "Gerelateerde zaak" });

            }

            public static ILocator GetZaakItem(this IPage page)
            {
                return page.GetByText("ZAAK-2023-001");
            }

            public static ILocator GetZaakCheckbox(this IPage page)
            {
                return page.GetByRole(AriaRole.Checkbox, new() { Name = "ZAAK-2023-001" });
            ;
        }

            public static ILocator GetSpecificVraagField(this IPage page)
            {
                return page.GetByRole(AriaRole.Textbox, new() { Name = "Specifieke vraag" });
            }

            public static ILocator GetAfhandelingField(this IPage page)
            {
                return page.GetByRole(AriaRole.Combobox, new() { Name = "Afhandeling" });
            }

            public static ILocator GetAfdelingField(this IPage page)
            {
                return page.Locator("input[type='search']");
            }

            public static ILocator GetKanaalField(this IPage page)
            {
                return page.GetByLabel("Kanaal");
            }
        public static ILocator GetOpslaanButton(this IPage page)
            {
                return page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
            }

            public static ILocator GetSuccessMessage(this IPage page)
            {
                  return page.Locator("output[role='status'].confirm");
            }

            public static ILocator GetContactmomentenTab(this IPage page)
            {
                return page.GetByRole(AriaRole.Tab, new() { Name = "Contactmomenten" });
            }

            public static ILocator GetFirstContactmoment(this IPage page)
            {
            return page.Locator("ul.overview > li:nth-child(2) summary");
        }
        public static ILocator GetFirstContactMomentSummary(this IPage page) => page.GetFirstContactmoment();
        public static ILocator GetMedewerkerHeader(this ILocator locator) => locator.Locator("span[aria-describedby='medewerker-header']");
        public static ILocator GetGespreksresultaatHeader(this ILocator locator) => locator.Locator("span[aria-describedby='gespreksresultaat-header']");
        public static ILocator GetAfdelingHeader(this ILocator locator) => locator.Locator("span[aria-describedby='afdeling-header']");
        public static ILocator GetDetailsList(this IPage page) => page.Locator("ul.overview > li:nth-child(2) dl");
        public static ILocator GetZaaknummer(this ILocator locator) => locator.Locator("dd:nth-of-type(2)");
        public static ILocator GetSpecifiekeVraag(this ILocator locator) => locator.Locator("dd:nth-of-type(3)");


    }
}


