using Microsoft.Playwright;

namespace PlaywrightTests
{
    [TestClass]
    public class GespreksresultatenTests : BaseTestInitializer
    {
        // dummy test to ensure that the base class functions properly from github actions
        [TestMethod]
        public async Task DummyTest()
        {
            await Expect(Page.Locator("a[href='/beheer']")).ToBeVisibleAsync();
        }


        [TestMethod]
        public async Task TestPaginationAsync()
        {
            // Locate the 'Nieuws' section
            var nieuwsSection = Page.Locator("section").Filter(new() { HasText = "Nieuws"});
            await Expect(nieuwsSection).ToBeVisibleAsync();

            // Locate the 'Next' page button using the pagination structure
            var nextPageButton = nieuwsSection.Locator("[rel='next']").First;
            await Expect(nextPageButton).ToBeVisibleAsync();

            // Click the 'Next' page button
            await nextPageButton.ClickAsync();

            // Wait for the button to ensure the page navigation has started
            await nextPageButton.WaitForAsync();

            // Verify that the first page button is still visible after navigation
            var firstPageButton = nieuwsSection.GetByLabel("Pagina 1");
            // TODO fix the pagination component. numbers should always have an aria label with the number in it
            await Expect(firstPageButton).ToBeVisibleAsync();

            // Verify that the current page button reflects the correct page number
            var currentPageButton = nieuwsSection.Locator("[aria-current=page]");
            var page2Button = nieuwsSection.GetByLabel("Pagina 2");
            var page2ButtonWithAriaCurrentPage = currentPageButton.And(page2Button);

            // Ensure the current page button's aria-label attribute is 'Pagina 2'
            await Expect(page2ButtonWithAriaCurrentPage).ToBeVisibleAsync();

        }


        [TestMethod]
        public async Task TestSkillsFilteringAsync()
        {
            // Example: Test filtering by skill
            var categorieFilterSection = Page.Locator("details").Filter(new() { HasText = "Filter op categorie" });
            await Expect(categorieFilterSection).ToBeVisibleAsync();
            await categorieFilterSection.Locator("summary").ClickAsync();
            var algemeenCheckbox = categorieFilterSection.GetByRole(AriaRole.Checkbox, new() { Name = "Algemeen" });
            var belastingenCheckbox = categorieFilterSection.GetByRole(AriaRole.Checkbox, new() { Name = "Belastingen" });
            
            await algemeenCheckbox.CheckAsync();
            await belastingenCheckbox.CheckAsync();

            // Verify results are filtered
            var articles = Page.GetByRole(AriaRole.Article);
            await Expect(articles.First).ToBeVisibleAsync();
            
            var resultCount = await articles.CountAsync();

            Assert.IsTrue(resultCount > 0, "Expected to find articles after filtering by skills.");

            // Loop through each article and verify it contains at least one of the selected skills
            for (var i = 0; i < resultCount; i++)
            {
                var article = articles.Nth(i);
                var algemeenSkill = article.Locator("small.category-Algemeen");
                var belastingenSkill = article.Locator("small.category-Belastingen");
                await Expect(algemeenSkill.Or(belastingenSkill).First).ToBeVisibleAsync();
            }

            // Reset filters
            await algemeenCheckbox.UncheckAsync();
            await belastingenCheckbox.UncheckAsync();
        }

        // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
        [TestMethod]
        private async Task TestMarkAsImportantAsync()
        {
            // Example: Mark a news item as important
            await Page.ClickAsync("a:has-text('Details')");
            await Page.CheckAsync("input[name='Belangrijk']");
            await Page.ClickAsync("button:has-text('Opslaan')");
            await Page.WaitForURLAsync("https://dev.kiss-demo.nl/Beheer/NieuwsEnWerkinstructies");

            // Verify the item is marked as important
            Assert.IsTrue(await Page.IsVisibleAsync("a:has-text('Belangrijk')"));
        }

        // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
        [TestMethod]
        private async Task TestAddAndVerifySkillAsync()
        {
            // Add a skill
            await Page.ClickAsync("a:has-text('Skills')");
            await Page.FillAsync("input[name='new-skill']", "Test Skill");
            await Page.ClickAsync("button:has-text('Voeg toe')");

            // Verify the skill appears in the dropdown
            await Page.ClickAsync("summary:has-text('Filter op categorie')");
            Assert.IsTrue(await Page.IsVisibleAsync("input[name='Test Skill']"));
        }

        // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
        [TestMethod]
        private async Task TestDeleteSkillAsync()
        {
            // Delete the skill
            await Page.ClickAsync("a:has-text('Skills')");
            await Page.ClickAsync("button:has-text('Verwijder')");

            // Verify the skill is removed
            await Page.ClickAsync("summary:has-text('Filter op categorie')");
            Assert.IsFalse(await Page.IsVisibleAsync("input[name='Test Skill']"));
        }

        // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
        [TestMethod]
        private async Task TestModifyAndVerifyNewsItemAsync()
        {
            // Modify an existing news item
            await Page.ClickAsync("a:has-text('Details')");
            await Page.FillAsync("textarea[name='editor']", "Updated Content");
            await Page.ClickAsync("button:has-text('Opslaan')");
            await Page.WaitForURLAsync("https://dev.kiss-demo.nl/Beheer/NieuwsEnWerkinstructies");

            // Verify the modification
            Assert.IsTrue(await Page.IsVisibleAsync("article:has-text('Updated Content')"));
        }
    }

}
