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

            await Page.CreateNewContactmomentAsync();

            await Step("User fills Lastname and Birthday and perform search");

            await Page.Personen_LastNameInput().FillAsync("Burck");
            await Page.Personen_BirthDateInput().FillAsync("17-11-1952");
            await Page.PersonenFirst_SearchButton().ClickAsync();
             
            await Step("Verify navigation to the Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");
        
        }

        
        [TestMethod("2. Searching by Last Name and Date of Birth (Not Found)")]
        public async Task SearchByLastNameAndDOB_NotFoundAsync()
        {

            await Step("Given the user is on the startpagina ");

            await Page.CreateNewContactmomentAsync();

            await Step("User fills Lastname and Birthday and perform search");

            await Page.Personen_LastNameInput().FillAsync("TestDB");
            await Page.Personen_BirthDateInput().FillAsync("11-12-1990");
            await Page.PersonenFirst_SearchButton().ClickAsync();

            await Step("The message is displayed as “Geen resultaten gevonden voor ’TestDB, 11-12-1990");
           
            await  Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'TestDB, 11-12-1990'.");

        }
        
        [TestMethod("3. Searching by BSN (Valid)")]
        public async Task SearchByBSN_Valid()
        {
            await Step("When user starts a new contactmoment");

            await Page.NavigateToContactMomentAsync();


            await Step(" Perform the search with a valid BSN");

            var bsnInput = Page.GetByRole(AriaRole.Textbox, new() { Name = "bsn" });

            await bsnInput.FillAsync("999992223");
            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Verify navigation to the Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

        }


        [TestMethod(" 4. Searching by BSN (Invalid)")]
        public async Task SearchByBSN_Invalid()
        {
            await Step("When user starts a new contactmoment");

            await Page.NavigateToContactMomentAsync();

            await Step(" Perform the search with a valid BSN");

            var bsnInput = Page.GetByRole(AriaRole.Textbox, new() { Name = "bsn" });

            await bsnInput.FillAsync("123456789");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Check for the error message");
             
            Assert.AreEqual( await bsnInput.EvaluateAsync<string>("(el) => el.validationMessage"), "Dit is geen valide BSN.");

          }

        #endregion
    }
}
