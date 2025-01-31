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

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'TestDB, 11-12-1990'.");

        }

        [TestMethod("3. Searching by BSN (Valid)")]
        public async Task SearchByBSN_Valid()
        {
            await Step("Given the user is on the startpagina ");

            await Page.CreateNewContactmomentAsync();

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
            await Step("Given the user is on the startpagina ");

            await Page.CreateNewContactmomentAsync();

            await Step(" Perform the search with a invalid BSN");

            var bsnInput = Page.GetByRole(AriaRole.Textbox, new() { Name = "bsn" });

            await bsnInput.FillAsync("123456789");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("The message is displayed as “Dit is geen valide BSN");

            Assert.AreEqual(await bsnInput.EvaluateAsync<string>("(el) => el.validationMessage"), "Dit is geen valide BSN.");

        }


        [TestMethod("5. Searching by Postcode and Huisnummer (Valid)")]
        public async Task SearchByPostcodeAndHuisnummer_Valid()
        {
            await Step("Given the user is on the startpagina ");

            await Page.CreateNewContactmomentAsync();

            await Step("Perform the search with valid postcode and huisnummer");

            var postCode = "3544 NG";
            var huisNummer = "10";

            await Page.Personen_PostCodeInput().FillAsync(postCode);
            await Page.Personen_HuisnummerInput().FillAsync(huisNummer);
            await Page.PersonenSecond_SearchButton().ClickAsync();


            await Step("Verify that multiple results are displayed in the table");

            await Page.GetByRole(AriaRole.Table).WaitForAsync();

            Assert.IsTrue(await Page.SearchAddressByPostalAndHuisNummer(postCode, huisNummer).CountAsync() > 1);
        }
        
        [TestMethod("6. Searching by Postcode and Huisnummer (Not Found)")]
        public async Task SearchByPostcodeAndHuisnummer_NotFound()
        {
            await Step("Given the user is on the startpagina");

            await Page.CreateNewContactmomentAsync();
         
            await Step(" Perform the search with non-existent data");
           
            await Page.Personen_PostCodeInput().FillAsync("3544 NG");
            await Page.Personen_HuisnummerInput().FillAsync("11");
            await Page.PersonenSecond_SearchButton().ClickAsync(); 

            await Step("Check for the error message");

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor '3544NG, 11'.");


        }

        #endregion
    }
}
