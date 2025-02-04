using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch
{
    [TestClass]
    public class ContactMomentSearchScenarios : KissPlaywrightTest
    {
        #region Test Cases

        [TestMethod("1. Searching by Last Name and Date of Birth (Valid)")]
        public async Task SearchByLastNameAndDOB_ValidAsync()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"Burck\" in the field achternaam and enters \"17-11-1952\" in the field geboortedatum ");

            await Page.Personen_LastNameInput().FillAsync("Burck");
            await Page.Personen_BirthDateInput().FillAsync("17-11-1952");

            await Step("And clicks the search button");

            await Page.PersonenFirst_SearchButton().ClickAsync();
             
            await Step("Then user is navigated to Persoonsinformatie page ");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");
        
        }

        
        [TestMethod("2. Searching by Last Name and Date of Birth (Not Found)")]
        public async Task SearchByLastNameAndDOB_NotFoundAsync()
        {

            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"TestDB” in the field achternaam and \"11-12-1990” in the field geboortedatum ");

            await Page.Personen_LastNameInput().FillAsync("TestDB");
            await Page.Personen_BirthDateInput().FillAsync("11-12-1990");

            await Step("And clicks the search button");

            await Page.PersonenFirst_SearchButton().ClickAsync();

            await Step("Then the message is displayed as “Geen resultaten gevonden voor ’TestDB, 11-12-1990’.");
           
            await  Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'TestDB, 11-12-1990'.");

        }

        #endregion
    }
}
