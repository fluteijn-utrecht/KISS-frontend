using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Kiss.Bff.EndToEndTest.AnonymousContactmomentZaak.Helpers;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentZaak
{
    [TestClass]
    public class AnonymousContactmomentZaakScenarios : KissPlaywrightTest
    {
        [TestMethod("1. Search for Zaak in Contactmoment")]
        public async Task SearchForZaakInContactmoment()
        {
            await Step("Given the user has started a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When the user clicks on 'Zaken' in the menu");

            await Page.GetZakenMenu().ClickAsync();

            await Step("And enters 'ZAAK-2023-001' in the search field");

            await Page.GetZaakSearchField().FillAsync("ZAAK-2023-001");

            await Step("And clicks zoeken (magnifying glass-icon)");

            await Page.GetZaakSearchButton().ClickAsync();

            await Step("Then the user will navigate to the screen 'Zaak ZAAK-2023-001'");

            await Expect(Page.GetZaakScreen("ZAAK-2023-001")).ToBeVisibleAsync();
        }

        [TestMethod("2. Register Contactmoment bij Zaak - I")]
        public async Task RegisterContactmomentBijZaakI()
        {
            await Step("Precondition: The scenario 'Search for Zaak in Contactmoment' has been executed");

            await SearchForZaakInContactmoment();

            await Step("Given the user is on the 'Zaak ZAAK-2023-001' screen");

            await Expect(Page.GetZaakScreen("ZAAK-2023-001")).ToBeVisibleAsync();

            await Step("When the user clicks Afronden in the Notes-Contactverzoek-Pane");

            await Page.GetPersonenAfrondenButton().ClickAsync();

            await Step("And the Afhandeling form is displayed");

            await Expect(Page.GetAfhandelingForm()).ToBeVisibleAsync();

            await Step("Then the heading 'Gerelateerde zaak' appears under 'Vraag 1'");

            await Expect(Page.GetGerelateerdeZaakHeading()).ToBeVisibleAsync();

            await Step("And the item 'ZAAK-2023-001' is listed");

            await Expect(Page.GetZaakItem()).ToBeVisibleAsync();

            await Step("And a checked checkbox is visible");

            await Expect(Page.GetZaakCheckbox()).ToBeCheckedAsync();
        }

        [TestMethod("3. Register Contactmoment bij Zaak - II")]
        public async Task RegisterContactmomentBijZaakII()
        {
            await Step("Precondition: Scenario: Register Contactmoment bij Zaak – I is executed");

            await RegisterContactmomentBijZaakI();

            await Step("Given the user is on the Afhandeling-form");

            await Expect(Page.GetAfhandelingForm()).ToBeVisibleAsync();

            await Step("When user enters value 'Contactmoment bij ZAAK-2023-001' in field Specifieke vraag");

            await Page.GetSpecificVraagField().FillAsync("Contactmoment bij ZAAK-2023-001");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And value 'Zelfstandig afgehandeld' in field Afhandeling");

            await Page.GetAfhandelingField().SelectOptionAsync(new[] { "Zelfstandig afgehandeld" });

            await Step("And selects value 'Parkeren' in field Afdeling");

            await Page.GetAfdelingField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed on the Startpagina");

            await Expect(Page.GetSuccessMessage()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("4. View Contactmoment bij Zaak")]
        public async Task ViewContactmomentBijZaak()
        {
            await Step("Precondition: Scenario: Register Contactmoment bij Zaak – II is executed");

            await RegisterContactmomentBijZaakII();

            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on 'Zaken' in the menu");

            await Page.GetZakenMenu().ClickAsync();

            await Step("And enters 'ZAAK-2023-001' in the search field");

            await Page.GetZaakSearchField().FillAsync("ZAAK-2023-001");

            await Step("And clicks zoeken");

            await Page.GetZaakSearchButton().ClickAsync();

            await Step("And navigates to the screen 'Zaak ZAAK-2023-001'");

            await Expect(Page.GetZaakScreen("ZAAK-2023-001")).ToBeVisibleAsync();

            await Step("And clicks on tab Contactmomenten");

            await Page.GetContactmomentenTab().ClickAsync();

            await Step("Then the first Contactmoment should contain the details as entered in Scenario: Register Contactmoment bij Zaak – II");

            var firstContactMomentSummary = Page.GetFirstContactMomentSummary();
            await Expect(firstContactMomentSummary.GetGespreksresultaatHeader()).ToHaveTextAsync("Zelfstandig afgehandeld");
            await Expect(firstContactMomentSummary.GetAfdelingHeader()).ToHaveTextAsync("Parkeren");

            await Page.Locator("ul.overview > li:nth-child(2) details").EvaluateAsync("el => el.open = true");

            var detailsList = Page.GetDetailsList();
            await Expect(detailsList.GetZaaknummer()).ToHaveTextAsync("ZAAK-2023-001");
            await Expect(detailsList.GetSpecifiekeVraag()).ToHaveTextAsync("Contactmoment bij ZAAK-2023-001");
        }
    }
    
}
