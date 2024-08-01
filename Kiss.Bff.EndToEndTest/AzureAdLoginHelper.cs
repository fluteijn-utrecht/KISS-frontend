using Microsoft.Playwright;
using OtpNet;
using System;
using System.Threading.Tasks;

namespace PlaywrightTests
{
    public class AzureAdLoginHelper
    {
        private readonly IPage _page;
        private readonly string _username;
        private readonly string _password;
        private readonly string _totpSecret;

        public AzureAdLoginHelper(IPage page, string username, string password, string totpSecret)
        {
            _page = page;
            _username = username;
            _password = password;
            _totpSecret = totpSecret;
        }

        public async Task LoginAsync()
        {
            await _page.GotoAsync("https://dev.kiss-demo.nl/");

            await _page.FillAsync("input[name='loginfmt']", _username);
            await _page.ClickAsync("input[type='submit']");

            await _page.WaitForSelectorAsync("input[name='passwd']");
            await _page.FillAsync("input[name='passwd']", _password);
            await _page.ClickAsync("input[type='submit']");

            await Handle2FAAsync();

            await _page.WaitForURLAsync("https://dev.kiss-demo.nl/**");
        }

        private async Task Handle2FAAsync()
        {
            // Wait for the TOTP input field to appear
            await _page.WaitForSelectorAsync("input[name='otc']", new() { Timeout = 30000 });

            // Generate TOTP code
            var totp = new Totp(Base32Encoding.ToBytes(_totpSecret));
            string totpCode = totp.ComputeTotp();

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
            var staySignedInPromptSelector = "input[value='Nee']";
            await Task.Delay(2000);
            var staySignedInButton = await _page.QuerySelectorAsync(staySignedInPromptSelector);

            // Handle the prompt if present
            if (staySignedInButton != null)
            {
                await staySignedInButton.ClickAsync();
            }

            // Wait for navigation or some confirmation that the login is successful
            await _page.WaitForNavigationAsync(new() { Timeout = 30000 });
        }

    }
}
