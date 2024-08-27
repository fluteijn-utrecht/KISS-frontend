using Microsoft.Playwright;
using OtpNet;

namespace PlaywrightTests
{
    public class AzureAdLoginHelper
    {
        private const int MaxTries = 9;
        private readonly IPage _page;
        private readonly string _username;
        private readonly string _password;
        private readonly Totp _totp;

        public AzureAdLoginHelper(IPage page, string username, string password, string totpSecret)
        {
            _page = page;
            _username = username;
            _password = password;
            _totp = new Totp(Base32Encoding.ToBytes(totpSecret));
        }

        public async Task LoginAsync()
        {
            await _page.GotoAsync("/");

            await _page.FillAsync("input[name='loginfmt']", _username);
            await _page.ClickAsync("input[type='submit']");

            await _page.WaitForSelectorAsync("input[name='passwd']");
            await _page.FillAsync("input[name='passwd']", _password);
            await _page.ClickAsync("input[type='submit']");

            await Handle2FAAsync();
        }

        private async Task Handle2FAAsync()
        {
            // Wait for the TOTP input field to appear
            await _page.WaitForSelectorAsync("input[name='otc']", new() { Timeout = 30000 });


            // Check for "Enter Manually" link and click if present
            var enterManuallyLink = _page.GetByRole(AriaRole.Link, new() { Name = "Microsoft Authenticator" });

            if (await enterManuallyLink.IsVisibleAsync())
            {
                await enterManuallyLink.ClickAsync();
            }

            var verifyButton = _page.GetByRole(AriaRole.Button, new() { Name = "Verifiëren" })
                .Or(_page.GetByRole(AriaRole.Button, new() { Name = "Verify" }));

            var declineStaySignedInButton = _page.GetByText("Nee").Or(_page.GetByText("No"));

            var homePageSelector = _page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });

            var totpBoxSelector = _page.GetByRole(AriaRole.Textbox, new() { Name = "Code" });

            for (var attempt = 1; attempt <= MaxTries; attempt++)
            {
                // we will either get the 2FA code input and the submit button,
                // or a decline button for staying signed in,
                // or we will go back to the home page
                await totpBoxSelector
                    .Or(verifyButton)
                    .Or(declineStaySignedInButton)
                    .Or(homePageSelector)
                    .First
                    .WaitForAsync();

                if (await homePageSelector.IsVisibleAsync())
                {
                    break;
                }

                if (await totpBoxSelector.IsVisibleAsync())
                {
                    // Generate TOTP code
                    var totpCode = _totp.ComputeTotp();
                    // Fill in the TOTP code
                    await totpBoxSelector.FillAsync(totpCode);
                }

                if (await verifyButton.IsVisibleAsync())
                {
                    await verifyButton.ClickAsync();
                }

                // check if the decline button for "Stay signed in?" is visible, if so, click it
                if (await declineStaySignedInButton.IsVisibleAsync())
                {
                    await declineStaySignedInButton.ClickAsync();
                }
            }

            await homePageSelector.WaitForAsync();
        }
    }
}
