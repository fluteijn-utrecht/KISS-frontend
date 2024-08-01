namespace PlaywrightTests
{
    [TestClass]
    public class GespreksresultatenTests : BaseTestInitializer
    {
        // dummy test to ensure that the base class functions properly from github actions
        [TestMethod]
        public async Task DummyTest()
        {
            var isEditor = await Page.IsVisibleAsync("a[href='/beheer']");
            Assert.IsTrue(isEditor, "User does not have editor role.");
        }
    }
}
