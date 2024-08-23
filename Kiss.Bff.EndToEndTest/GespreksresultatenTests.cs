using Microsoft.Playwright;
using static System.Net.Mime.MediaTypeNames;

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
            // Locate the 'Next' page button using the pagination structure
            var nextPageButton = Page.Locator("a.denhaag-pagination__link[rel='next']").First;
            bool hasNextPage = await nextPageButton.IsVisibleAsync();

            if (hasNextPage)
            {
                // Click the 'Next' page button
                await nextPageButton.ClickAsync();

                // Wait for the button to ensure the page navigation has started
                await nextPageButton.WaitForAsync();

                // Verify that the first page button is still visible after navigation
                var firstPageButton = Page.Locator("a.denhaag-pagination__link[aria-label='Pagina 1']");
                await Expect(firstPageButton).ToBeVisibleAsync();

                // Verify that the current page button reflects the correct page number
                var currentPageButton = Page.Locator("a.denhaag-pagination__link--current");
                await Expect(currentPageButton).ToBeVisibleAsync();

                // Ensure the current page button's aria-label attribute is 'Pagina 2'
                await Expect(currentPageButton).ToHaveAttributeAsync("aria-label", "Pagina 2");
            }
        }


        [TestMethod]
        public async Task TestSkillsFilteringAsync()
        {
            // Example: Test filtering by skill
            await Page.ClickAsync("summary:has-text('Filter op categorie')");
            await Page.CheckAsync("input[name='Algemeen']");
            await Page.CheckAsync("input[name='Belastingen']");

            // Verify results are filtered
            var articles = Page.Locator("article");
            int resultCount = await articles.CountAsync();
            Assert.IsTrue(resultCount > 0, "Expected to find articles after filtering by skills.");

            // Loop through each article and verify it contains at least one of the selected skills
            for (int i = 0; i < resultCount; i++)
            {
                var article = articles.Nth(i);
                bool hasAlgemeenSkill = await article.Locator("small.category-Algemeen").IsVisibleAsync();
                bool hasBelastingenSkill = await article.Locator("small.category-Belastingen").IsVisibleAsync();

                Assert.IsTrue(hasAlgemeenSkill || hasBelastingenSkill, $"Article {i + 1} does not contain the expected skills.");
            }

            // Reset filters
            await Page.UncheckAsync("input[name='Algemeen']");
            await Page.UncheckAsync("input[name='Belastingen']");
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
