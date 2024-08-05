using Microsoft.Playwright;
using OtpNet;

namespace PlaywrightTests
{
    public class AzureAdLoginHelper
    {
        private readonly IPage _page;
        private readonly Uri _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _totpSecret;

        public AzureAdLoginHelper(IPage page, Uri baseUrl, string username, string password, string totpSecret)
        {
            _page = page;
            _baseUrl = baseUrl;
            _username = username;
            _password = password;
            _totpSecret = totpSecret;
        }

        public async Task LoginAsync()
        {
            await _page.GotoAsync(_baseUrl.ToString());

            await _page.FillAsync("input[name='loginfmt']", _username);
            await _page.ClickAsync("input[type='submit']");

            await _page.WaitForSelectorAsync("input[name='passwd']");
            await _page.FillAsync("input[name='passwd']", _password);
            await _page.ClickAsync("input[type='submit']");

            await Handle2FAAsync();

            await _page.WaitForURLAsync($"{_baseUrl}**");
        }

        private async Task Handle2FAAsync()
        {
            // Wait for the TOTP input field to appear
            await _page.WaitForSelectorAsync("input[name='otc']", new() { Timeout = 30000 });

            // Generate TOTP code
            var totp = new Totp(Base32Encoding.ToBytes(_totpSecret));
            var totpCode = totp.ComputeTotp();

            // Check for "Enter Manually" link and click if present
            var enterManuallyLink = await _page.QuerySelectorAsync("a:has-text('Ik kan mijn Microsoft Authenticator-app op dit moment niet gebruiken')");
            if (enterManuallyLink != null)
            {
                await enterManuallyLink.ClickAsync();
            }

            // Fill in the TOTP code
            await _page.FillAsync("input[name='otc']", totpCode);
            await _page.ClickAsync("input[type='submit']");

            // Wait for potential "Stay signed in?" prompt
            var declineStaySignedInButton = _page.GetByText("Nee").Or(_page.GetByText("No"));
            var homePageSelector = _page.GetByText("Nieuw contactmoment");

            // we will either get a decline button, or we will go back to the home page
            await declineStaySignedInButton.Or(homePageSelector).WaitForAsync();

            // check if the decline button is visible, if so, click it
            if (await declineStaySignedInButton.IsVisibleAsync())
            {
                await declineStaySignedInButton.ClickAsync();
            }
        }

    }
}
