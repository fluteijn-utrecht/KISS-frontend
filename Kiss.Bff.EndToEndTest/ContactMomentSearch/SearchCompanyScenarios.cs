using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch
{
    [TestClass]
    public class SearchCompanyScenarios : KissPlaywrightTest
    {


        [TestMethod("1. Searching by Partial Last Name and Date of Birth (Single Result)")]
        public async Task SearchByBedrijfsnaam_MultipleRecordsAsync()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters “Donald” in the field Bedrijfsnaam");

            var lastName = "Donald";

            await Page.Company_BedrijfsnaamInput().FillAsync(lastName);

            await Step("And clicks the search button");

            await Page.Company_FirstSearchButton().ClickAsync();

            await Step("Then the list displays multiple records associated with the name “Donald”.");

            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

            var resultCount = await Page.GetByRole(AriaRole.Table).GetByRole(AriaRole.Row).CountAsync();

            Assert.IsTrue(resultCount > 2, $"Expected multiple records associated with the last name '{lastName}', but found {resultCount}.");
        }

        [TestMethod("2. Search By Bedrijfsnaam Unique Result ")]
        public async Task SearchByBedrijfsnaam_UniqueResultAsync()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters “Test Stichting Bolderbast” in the field Bedrijfsnaam");

            await Page.Company_BedrijfsnaamInput().FillAsync("Test Stichting Bolderbast");

            await Step("And clicks the search button");

            await Page.Company_FirstSearchButton().ClickAsync();

            await Step("Then user is navigated to bedrijfsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Bedrijfsinformatie" })).ToBeVisibleAsync();

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Bedrijfsinformatie" })).ToHaveTextAsync("Bedrijfsinformatie");
        }

    }
}
