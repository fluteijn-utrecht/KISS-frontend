using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmoment.Helpers
{
    internal static class ManageAnonymousContactmoment
    {
        public static async Task CreateNewContactmomentAsync(this IPage page)
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }).ClickAsync();
        }
    }
}
