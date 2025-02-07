
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers; 

namespace Kiss.Bff.EndToEndTest.AfhandelingForm
{

    [TestClass]
    public class AfhandelingFormScenarios : KissPlaywrightTest
    {

        [TestMethod("1: Validation of Text in Notitieblok")]
        public async Task TestValidationOfTextInNotitieblok()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters “test notitieblok” in Notitieblok");

            var note = "test notitieblok";

            await Page.GetPersonenNotitieblokTextbox().FillAsync(note);

            await Step("Click the Afronden button");

            await Page.GetPersonenAfrondenButton().ClickAsync();

            await Step("Then Afhandeling form has value as “test notitieblok” in field Notitie");

            await Expect(Page.GetAfhandelingNotitieTextBox()).ToHaveValueAsync(note);
        }

    }
}
