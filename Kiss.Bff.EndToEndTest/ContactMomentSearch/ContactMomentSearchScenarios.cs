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
            await Step("When user starts a new contactmoment");

            await Page.NavigateToContactMomentAsync();

            await Step("Perform the search");

            await Page.Personen_LastNameInput().FillAsync("Burck");
            await Page.Personen_BirthDateInput().FillAsync("17-11-1952");
            await Page.PersonenFirst_SearchButton().ClickAsync();
             
            await Step("Verify navigation to the Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");
        
        }

        
        [TestMethod("2. Searching by Last Name and Date of Birth (Not Found)")]
        public async Task SearchByLastNameAndDOB_NotFoundAsync()
        {

            await Step("When user starts a new contactmoment");

            await Page.NavigateToContactMomentAsync();

            await Step("Perform the search");

            await Page.Personen_LastNameInput().FillAsync("TestDB");
            await Page.Personen_BirthDateInput().FillAsync("11-12-1990");
            await Page.PersonenFirst_SearchButton().ClickAsync();

            await Step("Check for the error message");
           
            await  Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'TestDB, 11-12-1990'.");

        }

        #endregion
    }
}
