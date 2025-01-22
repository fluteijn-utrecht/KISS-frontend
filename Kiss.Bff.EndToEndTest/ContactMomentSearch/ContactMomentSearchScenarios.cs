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

            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }).ClickAsync();

            await Step("Perform the search");
            
            var searchButton = Page.Locator("form").Filter(new() { HasText = "Achternaam Geboortedatum" }).GetByRole(AriaRole.Button);
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Achternaam" }).FillAsync("Burck");
            await Page.GetByLabel("Geboortedatum").FillAsync("17-11-1952");
            await searchButton.ClickAsync();

            await Step("Verify navigation to the Persoonsinformatie page");

            await Page.WaitForNavigationAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");
        
        }

        
        [TestMethod("2. Searching by Last Name and Date of Birth (Not Found)")]
        public async Task SearchByLastNameAndDOB_NotFoundAsync()
        {

            await Step("When user starts a new contactmoment");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }).ClickAsync();

            await Step("Perform the search");

            var searchButton = Page.Locator("form").Filter(new() { HasText = "Achternaam Geboortedatum" }).GetByRole(AriaRole.Button);
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Achternaam" }).FillAsync("TestDB");
            await Page.GetByLabel("Geboortedatum").FillAsync("11-12-1990");
            await searchButton.ClickAsync();

            await Step("Check for the error message");
           
            await  Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'TestDB, 11-12-1990'.");
 

             
        }

        #endregion
    }
}
