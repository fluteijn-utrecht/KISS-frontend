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
    }
}
