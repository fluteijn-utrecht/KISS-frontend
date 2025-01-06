using Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers;

namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies;

[TestClass]
public class Scenarios : BaseTestInitializer
{
    [TestMethod]
    public async Task Scenario01()
    {
        await Step("Given there is at least 1 nieuwsbericht");
        await using var news = await Page.CreateBericht(new() { Titel = "Playwright test nieuwsbericht", BerichtType = BerichtType.Nieuws });

        await Step("And there is at least 1 werkinstructie");
        await using var werkbericht = await Page.CreateBericht(new() { Titel = "Playwright test werkinstructie", BerichtType = BerichtType.Werkinstructie });

        await Step("When the user navigates to the HOME Page");
        await Page.GotoAsync("/");

        await Step("Then nieuwsberichten are displayed");
        await Expect(Page.GetNieuwsSection().GetByRole(AriaRole.Article).First).ToBeVisibleAsync();

        await Step("And werkinstructies are displayed");
        await Expect(Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article).First).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario02()
    {
        await Step("Given there is at least 1 important message");
        await using var testbericht = await Page.CreateBericht(new() { Titel = "Playwright test bericht belangrijk", IsBelangrijk = true });

        await Step("When navigates through the HOME Page");
        await Page.GotoAsync("/");

        await Step("Then the count of the important messages is displayed in the News and Instructions tabs.");
        var count = await GetFeaturedCount();
        Assert.AreNotEqual(0, count);
    }

    [TestMethod]
    public async Task Scenario03()
    {
        await Step("Given there is at least 1 nieuwsbericht");
        await using var testbericht = await Page.CreateBericht(new() { Titel = "Playwright test bericht", Inhoud = "Inhoud die we gaan verbergen" });
        var article = Page.GetBerichtOnHomePage(testbericht);
        var markeerGelezenButton = article.GetByRole(AriaRole.Button).And(article.GetByTitle("Markeer als gelezen"));
        var markeerOngelezenButton = article.GetByRole(AriaRole.Button).And(article.GetByTitle("Markeer als ongelezen"));
        var body = article.GetByText(testbericht.Inhoud!);

        await Step("When the user navigates through the HOME Page");
        await Page.GotoAsync("/");

        await Step("And clicks on the book icon within the nieuwsbericht card ");
        await markeerGelezenButton.ClickAsync();

        await Step("Then the button title on hover changes to 'markeer ongelezen'");
        await Expect(markeerGelezenButton).ToBeHiddenAsync();
        await Expect(markeerOngelezenButton).ToBeVisibleAsync();

        await Step("And the body of the nieuwsbericht is hidden");
        await Expect(body).ToBeHiddenAsync();
    }

    [TestMethod]
    public async Task Scenario04()
    {
        var newsArticles = Page.GetNieuwsSection().GetByRole(AriaRole.Article);

        await Step("Given there are at least 20 nieuwsberichten");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x
            });
        await using var berichten = await Page.CreateBerichten(berichtRequests);

        await Step("When the user navigates through the HOME Page");
        await Page.GotoAsync("/");

        var initialFirstArticleAriaSnapshot = await newsArticles.First.AriaSnapshotAsync();

        await Step("And clicks on the \"Next\" button to go to the next page");
        var nextPageButton = Page.GetNieuwsSection().Locator("[rel='next']").First;
        await nextPageButton.ClickAsync();

        await Step("Then the user should see 10 new articles on the next page");
        await Expect(newsArticles).ToHaveCountAsync(10);
        await Expect(newsArticles.First).Not.ToMatchAriaSnapshotAsync(initialFirstArticleAriaSnapshot);

        await Step("And the current page number should be 2");
        var currentPageButton = Page.GetNieuwsSection().Locator("[aria-current=page]");
        var page2Button = Page.GetNieuwsSection().GetByLabel("Pagina 2");
        var aButtonThatIsTheCurrentPageAndHasLabelPagina2 = currentPageButton.And(page2Button);
        await Expect(aButtonThatIsTheCurrentPageAndHasLabelPagina2).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario05()
    {
        var newSection = Page.GetNieuwsSection();
        var newsArticles = Page.GetNieuwsSection().GetByRole(AriaRole.Article);

        await Step("Given there are at least 20 nieuwsberichten");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x
            });
        await using var berichten = await Page.CreateBerichten(berichtRequests);

        await Step("And the user is on the HOME page");
        await Page.GotoAsync("/");
        await Expect(Page.GetNieuwsSection()).ToBeVisibleAsync();

        // Locate the 'Next' page button using the pagination structure
        await Step("And is on page 2 with 10 articles displayed");
        var nextPageButton = Page.GetNieuwsSection().Locator("[rel='next']").First;
        await nextPageButton.ClickAsync();
        await Expect(newsArticles).ToHaveCountAsync(10);

        var intialAriaSnapshot = await newsArticles.First.AriaSnapshotAsync();

        await Step("When the user clicks on the \"Previous\" button");
        var previousPageButton = Page.GetNieuwsSection().Locator("[rel='prev']").First;
        await previousPageButton.ClickAsync();

        await Step("Then the user should see 10 different articles on the first page");
        await Expect(newsArticles).ToHaveCountAsync(10);
        await Expect(newsArticles.First).Not.ToMatchAriaSnapshotAsync(intialAriaSnapshot);

        await Step("And the current page number should be 1");
        var currentPageButton = Page.GetNieuwsSection().Locator("[aria-current=page]");
        var page1Button = Page.GetNieuwsSection().GetByLabel("Pagina 1");
        var aButtonThatIsTheCurrentPageAndHasLabelPagina1 = currentPageButton.And(page1Button);
        await Expect(aButtonThatIsTheCurrentPageAndHasLabelPagina1).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario06()
    {
        await Step("Given there are at least 20 werkinstructies");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x,
                BerichtType = BerichtType.Werkinstructie
            });
        await using var berichten = await Page.CreateBerichten(berichtRequests);
        var articles = Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article);

        await Step("And the user is on the HOME Page");
        await Page.GotoAsync("/");
        await Expect(Page.GetWerkinstructiesSection()).ToBeVisibleAsync();

        await Step("And is on page 2 with 10 werkinstructies displayed");
        var nextPageButton = Page.GetWerkinstructiesSection().Locator("[rel='next']").First;
        await nextPageButton.ClickAsync();
        await Expect(articles).ToHaveCountAsync(10);

        var intialFirstArticleAriaSnapshot = await articles.First.AriaSnapshotAsync();

        await Step("When the user clicks on the \"Previous\" button");
        var previousPageButton = Page.GetWerkinstructiesSection().Locator("[rel='prev']").First;
        await previousPageButton.ClickAsync();

        await Step("Then the user should see 10 different werkinstructies on the first page");
        await Expect(articles).ToHaveCountAsync(10);
        await Expect(articles.First).Not.ToMatchAriaSnapshotAsync(intialFirstArticleAriaSnapshot);

        await Step("And the current page number should be 1");
        var currentPageButton = Page.GetWerkinstructiesSection().Locator("[aria-current=page]");
        var page1Button = Page.GetWerkinstructiesSection().GetByLabel("Pagina 1");
        var aButtonThatIsTheCurrentPageAndHasLabelPagina1 = currentPageButton.And(page1Button);
        await Expect(aButtonThatIsTheCurrentPageAndHasLabelPagina1).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario07()
    {
        await Step("Given there are at least 20 werkinstructies");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x,
                BerichtType = BerichtType.Werkinstructie
            });

        await using var berichten = await Page.CreateBerichten(berichtRequests);
        var articles = Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article);

        await Step("And the user is on the HOME Page");
        await Page.GotoAsync("/");
        await Expect(Page.GetWerkinstructiesSection()).ToBeVisibleAsync();

        // Locate the 'Next' page button using the pagination structure
        var nextPageButton = Page.GetWerkinstructiesSection().Locator("[rel='next']").First;
        var werkinstructies = Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article);

        await Step("And is on the last page of werkinstructies");

        // keep clicking on the next page button until it's disabled
        while (!await IsDisabledPage(nextPageButton))
        {
            await nextPageButton.ClickAsync();
            await werkinstructies.First.WaitForAsync();
        }

        var initialFirstArticleAriaSnapshot = await articles.First.AriaSnapshotAsync();

        await Step("When the user clicks on the \"Next\" button");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => nextPageButton.ClickAsync(new() { Timeout = 1000 }),
            "Expected the button to not be clickable, but it was");

        await Step("Then the user remains on the last page");

        await Step("And no additional werkinstructies are displayed");
        await Expect(articles.First).ToMatchAriaSnapshotAsync(initialFirstArticleAriaSnapshot);
    }

    [TestMethod]
    public async Task Scenario08()
    {
        await Step("Given there is a nieuwsbericht that is read");
        await using var bericht = await Page.CreateBericht(new (){ Titel = "Bericht playwright gelezen/ongelezen", Inhoud = "Text to look for" });
        await Page.GotoAsync("/");
        var article = Page.GetBerichtOnHomePage(bericht);
        var articleBody = article.GetByText(bericht.Inhoud);
        var markeerGelezenButton = article.GetByRole(AriaRole.Button).And(article.GetByTitle("Markeer als gelezen"));
        var markeerOngelezenButton = article.GetByRole(AriaRole.Button).And(article.GetByTitle("Markeer als ongelezen"));
        var articleHeading = article.GetByRole(AriaRole.Heading);
        await markeerGelezenButton.ClickAsync();
        await Expect(articleBody).ToBeHiddenAsync();

        await Step("And the user is on the HOME Page");
        await Page.GotoAsync("/");

        await Step("When the user clicks the 'Markeer als ongelezen' button");
        await markeerOngelezenButton.ClickAsync();

        await Step("Then content of the nieuwsbericht is visible");
        await Expect(article).ToContainTextAsync(bericht.Inhoud);
    }

    [TestMethod]
    public async Task Scenario09()
    {
        await Step("Given there are at least two skills");
        await using var skill1 = await Page.CreateSkill(Guid.NewGuid().ToString());
        await using var skill2 = await Page.CreateSkill(Guid.NewGuid().ToString());

        await Step("And there is exactly one nieuwsbericht related to the first skill");
        await using var berichtWithSkill1 = await Page.CreateBericht(new CreateBerichtRequest { Titel = Guid.NewGuid().ToString(), Skill = skill1.Naam });

        await Step("And there is exactly one nieuwsbericht related to the second skill");
        await using var berichtWithSkill2 = await Page.CreateBericht(new CreateBerichtRequest { Titel = Guid.NewGuid().ToString() });

        await Step("And there is at least one nieuwsbericht without a relation to any skill");
        await using var berichtWithoutSkill = await Page.CreateBericht(new CreateBerichtRequest { Titel = Guid.NewGuid().ToString(), Skill = skill2.Naam });

        await Step("And the user is on the HOME Page");
        await Page.GotoAsync("/");

        await Step("When the user selects the first skill from the filter options");
        await Page.GetSkillsSummaryElement().ClickAsync();
        await Page.GetSkillsFieldset().GetByRole(AriaRole.Checkbox, new() { Name = skill1.Naam }).CheckAsync();

        await Step("Then only the article related to the first skill is visible");
        var articles = Page.GetNieuwsSection().GetByRole(AriaRole.Article);
        await Expect(articles).ToHaveCountAsync(1);
        await Expect(articles.GetByRole(AriaRole.Heading, new() { Name = berichtWithSkill1.Titel })).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario10()
    {
        await Step("Given there is a skill that is not linked to any article");
        await using var skill = await Page.CreateSkill(Guid.NewGuid().ToString());

        await Step("And the user is on the HOME Page");
        await Page.GotoAsync("/");

        await Step("When the user selects the skill from the filter options");
        await Page.GetSkillsSummaryElement().ClickAsync();
        await Page.GetSkillsFieldset().GetByRole(AriaRole.Checkbox, new() { Name = skill.Naam }).CheckAsync();

        await Step("Then no articles are visible");
        // wait until the spinner is gone
        await Expect(Page.Locator(".spinner")).ToBeHiddenAsync();
        var articles = Page.GetByRole(AriaRole.Article);
        await Expect(articles).ToBeHiddenAsync();
    }

    // Dit test Stap 2. 8. 9. 10. 15. 
    //[TestMethod]
    //public async Task Als_ik_een_oud_bericht_update_komt_deze_bovenaan()
    //{
    //    try
    //    {
    //        // Check if old test messages exist
    //        var oldTestMessageLocator = Page.Locator("article:has-text('8e600d44-81fb-4302-9675-31b687619026')");
    //        if (await oldTestMessageLocator.IsVisibleAsync())
    //        {
    //            await DeleteBericht("8e600d44-81fb-4302-9675-31b687619026");
    //            await DeleteBericht("724e44a3-6ba1-4e92-85c3-d44e35238f4a");
    //            await DeleteBericht("5b8277a7-fb1a-4358-8099-24b9487b29bc");
    //        }


    //        // Step 2: Create Message A with the publish date one minute in the past
    //        await CreateBericht("Message A: 8e600d44-81fb-4302-9675-31b687619026", false, "", TimeSpan.FromMinutes(-1));

    //        // Create Message B and C with the current publish date
    //        await CreateBericht("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a", false, "");
    //        await CreateBericht("Important Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc", true, "");

    //        // Go to the page and retrieve the order of articles
    //        await Page.GotoAsync("/");

    //        await Page.WaitForSelectorAsync("article:has-text('Message A')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message B')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message C')");

    //        var allArticles = NieuwsSection.GetByRole(AriaRole.Article);

    //        // Dictionary to hold article positions
    //        var initialOrderOnPage = new Dictionary<string, int>();

    //        for (var index = 0; index < await allArticles.CountAsync(); index++)
    //        {
    //            var element = allArticles.Nth(index);
    //            var innerHtml = await element.InnerTextAsync();

    //            if (innerHtml.Contains("Message A: 8e600d44-81fb-4302-9675-31b687619026"))
    //            {
    //                initialOrderOnPage.Add("Message A", index);
    //            }
    //            if (innerHtml.Contains("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a"))
    //            {
    //                initialOrderOnPage.Add("Message B", index);
    //            }
    //            if (innerHtml.Contains("Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc"))
    //            {
    //                initialOrderOnPage.Add("Message C", index);
    //            }
    //        }

    //        var indexVanA = initialOrderOnPage["Message A"];
    //        var indexVanB = initialOrderOnPage["Message B"];
    //        var indexVanC = initialOrderOnPage["Message C"];

    //        Assert.IsTrue(indexVanC < indexVanB && indexVanB < indexVanA, "Initial order should be C, B, A.");

    //        // Act: Update message A
    //        await UpdateBericht("Message A: 8e600d44-81fb-4302-9675-31b687619026", "Updated Message A: 8e600d44-81fb-4302-9675-31b687619026");

    //        // Refresh page and retrieve articles again
    //        await Page.GotoAsync("/");

    //        await Page.WaitForSelectorAsync("article:has-text('Message A')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message B')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message C')");

    //        allArticles = NieuwsSection.GetByRole(AriaRole.Article);

    //        // Rebuild the dictionary for updated positions
    //        var orderOnPageAfterMessageUpdate = new Dictionary<string, int>();
    //        for (var index = 0; index < await allArticles.CountAsync(); index++)
    //        {
    //            var element = allArticles.Nth(index);
    //            var innerHtml = await element.InnerTextAsync();

    //            if (innerHtml.Contains("Updated Message A: 8e600d44-81fb-4302-9675-31b687619026"))
    //            {
    //                orderOnPageAfterMessageUpdate.Add("Message A", index);
    //            }
    //            if (innerHtml.Contains("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a"))
    //            {
    //                orderOnPageAfterMessageUpdate.Add("Message B", index);
    //            }
    //            if (innerHtml.Contains("Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc"))
    //            {
    //                orderOnPageAfterMessageUpdate.Add("Message C", index);
    //            }
    //        }

    //        // Assert the updated order: C (highest), B, A (lowest)
    //        indexVanA = orderOnPageAfterMessageUpdate["Message A"];
    //        indexVanB = orderOnPageAfterMessageUpdate["Message B"];
    //        indexVanC = orderOnPageAfterMessageUpdate["Message C"];

    //        Assert.IsTrue(indexVanC < indexVanB && indexVanB > indexVanA, "Updated order should be C, A, B.");
    //    }
    //    finally
    //    {
    //        // Clean up test messages
    //        await DeleteBericht("8e600d44-81fb-4302-9675-31b687619026");
    //        await DeleteBericht("724e44a3-6ba1-4e92-85c3-d44e35238f4a");
    //        await DeleteBericht("5b8277a7-fb1a-4358-8099-24b9487b29bc");
    //    }
    //}

    //// 9. Publiceer een bericht met markering Belangrijk 
    //[TestMethod]
    //public async Task Als_ik_een_belangrijk_bericht_publiceer_komt_deze_bovenaan()
    //{
    //    var titel = $"End to end test {Guid.NewGuid()}";
    //    // Step 1: Get the initial featured indicator count
    //    var initialFeatureCount = await GetFeaturedCount();

    //    // Step 2: Create a new important message
    //    await CreateBericht(titel, true, "");

    //    try
    //    {
    //        // Step 3: Go to the page and ensure the news section is visible
    //        await Page.GotoAsync("/");

    //        await Expect(NieuwsSection).ToBeVisibleAsync();

    //        // Step 4: Check if the newly created important message appears at the top
    //        var firstArticle = NieuwsSection.GetByRole(AriaRole.Article).First;
    //        await Expect(firstArticle).ToContainTextAsync(titel);
    //        var isBelangrijk = await firstArticle.Locator(".featured").IsVisibleAsync();

    //        // Ensure the first article contains "Belangrijk" only if it's supposed to
    //        if (isBelangrijk)
    //        {
    //            await Expect(firstArticle.Locator(".featured")).ToContainTextAsync("Belangrijk");
    //        }
    //        else
    //        {
    //            Console.WriteLine("This article does not contain the 'Belangrijk' tag.");
    //        }

    //        // Step 5: Get the new featured count
    //        var updatedCount = await GetFeaturedCount();
    //        Assert.IsTrue(updatedCount >= initialFeatureCount + 1, $"Expected featured count to be at least {initialFeatureCount + 1}, but got {updatedCount}");

    //        // Step 6: Mark the article as read
    //        await firstArticle.GetByRole(AriaRole.Button, new() { Name = "Markeer als gelezen" }).ClickAsync();

    //        // Step 7: Validate that the featured count is now back to the initial count
    //        var reUpdatedCount = await GetFeaturedCount();
    //        Assert.IsTrue(reUpdatedCount == initialFeatureCount, $"Expected featured count to be equal to the initial count {initialFeatureCount} again, but instead got {reUpdatedCount}");
    //    }
    //    finally
    //    {
    //        // Step 8: Clean up by deleting the created message
    //        await DeleteBericht(titel);
    //    }
    //}

    private async Task<int> GetFeaturedCount()
    {
        // Declare featuredIndicator outside the try block so it's accessible throughout the method
        var featuredIndicator = Page.Locator(".featured-indicator");
        await Page.WaitForResponseAsync(x => x.Url.Contains("featuredcount"));
        if (await featuredIndicator.IsVisibleAsync())
        {
            var featureText = await featuredIndicator.TextContentAsync();
            return int.Parse(featureText!);
        }
        return 0;
    }


    //// This test covers Step 12. 13. 14.
    //[TestMethod]
    //public async Task Als_ik_een_skill_toevoeg_wordt_deze_vermeld_in_de_filter()
    //{
    //    // Define the new skill name to be added and tested
    //    var newSkill = "Playwright Test Skill";

    //    try
    //    {
    //        // Step 1: Navigate to the Skills management page
    //        await NavigateToSkillsBeheer();

    //        // Step 2: Add the new skill
    //        await CreateSkill(newSkill);
    //        await Page.GotoAsync("/");
    //        // Step 3: Open the filter dropdown to verify the skill
    //        await Page.ClickAsync("summary:has-text('Filter op categorie')");

    //        // Step 4: Verify the newly added skill appears in the filter list as a checkbox option
    //        var addedSkillCheckbox = Page.GetByRole(AriaRole.Checkbox, new() { Name = newSkill }).First;
    //        await Expect(addedSkillCheckbox).ToBeVisibleAsync();

    //    }
    //    finally
    //    {
    //        // clean-up: Remove the skill after test completion
    //        await DeleteSkill(newSkill);
    //    }
    //}

    //// Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
    //[TestMethod]
    //public async Task Als_ik_een_skill_en_nieuws_item_toevoeg_zou_ik_deze_moeten_zien_bij_filteren()
    //{
    //    var newSkill = "Test Skill";
    //    var newsTitle = "Test Nieuws Item";
    //    bool isImportant = false;

    //    try
    //    {
    //        // Step 1: Create a new skill
    //        await CreateSkill(newSkill);

    //        // Step 2: Create a news item with the new skill
    //        await CreateBericht(newsTitle, isImportant, newSkill);

    //        // Step 3: Verify that the news item appears when filtering by the new skill
    //        await Page.GotoAsync("/");

    //        await Page.ClickAsync("summary:has-text('Filter op categorie')"); // Open the filter dropdown
    //        var skillCheckbox = Page.GetByRole(AriaRole.Checkbox, new() { Name = newSkill }).First;
    //        await skillCheckbox.CheckAsync(); // Check the skill in the filter

    //        // Step 4: Verify the news item appears
    //        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = newsTitle })).ToBeVisibleAsync();
    //    }
    //    finally
    //    {
    //        await DeleteBericht(newsTitle);
    //        await DeleteSkill(newSkill);
    //    }
    //}

    private ILocator NieuwsSection => Page.Locator("section").Filter(new() { HasText = "Nieuws" });

    private async Task MarkAllNewsItems(bool read)
    {
        // Locate the 'Nieuws' section
        await Expect(NieuwsSection).ToBeVisibleAsync();

        var firstGelezenButton = NieuwsSection.GetByTitle("Markeer als gelezen").First;
        var firstOnGelezenButton = NieuwsSection.GetByTitle("Markeer als ongelezen").First;

        var buttonToClick = read
            ? firstGelezenButton
            : firstOnGelezenButton;

        var firstPage = NieuwsSection.GetByRole(AriaRole.Link).Filter(new() { HasTextRegex = new("^1$") });

        if (!await IsDisabledPage(firstPage))
        {
            await firstPage.ClickAsync();
        }

        while (true)
        {
            await Expect(firstOnGelezenButton.Or(firstGelezenButton).First).ToBeVisibleAsync();

            // Mark all news items as read on the current page
            while (await buttonToClick.IsVisibleAsync())
            {
                await buttonToClick.ClickAsync();
            }

            var nextPage = NieuwsSection.Locator("[rel='next']").First;

            if (await IsDisabledPage(nextPage))
            {
                break;
            }

            await nextPage.ClickAsync();
        }

        if (!await IsDisabledPage(firstPage))
        {
            await firstPage.ClickAsync();
        }
    }

    private async Task UpdateBericht(string oldTitle, string newTitle)
    {
        // Navigate to the news management page
        await Page.NavigateToNieuwsWerkinstructiesBeheer();

        // Find the news item by its old title
        var nieuwsRows = Page.GetByRole(AriaRole.Row)
            .Filter(new()
            {
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = oldTitle, Exact = true })
            });

        // Click the "Details" link for the news item
        await nieuwsRows.GetByRole(AriaRole.Link, new() { Name = "Details" }).ClickAsync();

        // Update the title to the new one
        await Page.GetByLabel("Titel").FillAsync(newTitle);

        // Save the changes
        await Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
    }

    private async Task<bool> IsDisabledPage(ILocator locator)
    {
        await Expect(locator).ToBeVisibleAsync();

        var classes = await locator.GetAttributeAsync("class");
        if (classes == null) return false;
        // when the page is disabled it doesn't get a disabled attribute.
        // TODO: research if there is a better pattern for this that is compatible with the den haag pagination component:
        // https://nl-design-system.github.io/denhaag/?path=/docs/react-navigation-pagination--docs
        // Note that their component renders invalid html: a button with a rel attribute.
        // this might serve as a source of inspiration:
        // https://design.homeoffice.gov.uk/components?name=Pagination
        // https://design-system.w3.org/components/pagination.html
        return classes.Contains("denhaag-pagination__link--disabled")
            || classes.Contains("denhaag-pagination__link--current");
    }
}
