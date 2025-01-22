using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class CreateBerichtExtension
    {
        public static async Task<IAsyncDisposable> CreateBerichten(this IPage page, IEnumerable<CreateBerichtRequest> requests)
        {
            var berichten = new List<Bericht>();
            async ValueTask Dispose()
            {
                if (berichten.Count == 0) return;
                await page.Context.Tracing.GroupEndAsync();
                await page.Context.Tracing.GroupAsync("Cleanup berichten");
                await page.Context.Tracing.GroupAsync("Start cleanup");
                foreach (var item in berichten)
                {
                    try
                    {
                        await item.DisposeAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
                await page.Context.Tracing.GroupEndAsync();
            }
            try
            {
                foreach (var item in requests)
                {
                    var bericht = await page.CreateBericht(item);
                    berichten.Add(bericht);
                }
                return new DoOnDisposeAsync(Dispose);
            }
            catch (Exception)
            {
                await Dispose();
                throw;
            }
        }

        private class DoOnDisposeAsync(Func<ValueTask> thingToDo) : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => thingToDo();
        }

        public static async Task<Bericht> CreateBericht(this IPage page, CreateBerichtRequest request)
        {
            request = request with { Body = !string.IsNullOrWhiteSpace(request.Body) ? request.Body : request.Title };
            await page.NavigateToNieuwsWerkinstructiesBeheer();
            var toevoegenLink = page.GetByRole(AriaRole.Link, new() { Name = "Toevoegen" });
            await toevoegenLink.ClickAsync();
            await page.GetByRole(AriaRole.Radio, new() { Name = request.BerichtType.ToString() }).CheckAsync();

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(request.Title);

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Rich Text Editor" }).FillAsync(request.Body);

            if (request.IsImportant)
            {
                await page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" }).CheckAsync();
            }

            if (!string.IsNullOrEmpty(request.Skill))
            {
                var skillCheckbox = page.GetByRole(AriaRole.Checkbox, new() { Name = request.Skill });
                await skillCheckbox.CheckAsync(); // Ensure the skill checkbox is checked
            }

            // Use the current time as the base publish date
            var publishDate = DateTime.Now;

            // Apply the provided offset to the publish date
            if (request.PublishDateOffset.HasValue)
            {
                publishDate = publishDate.Add(request.PublishDateOffset.Value);
            }

            // Set the publish date in the input field
            var publishDateInput = page.GetByLabel("Publicatiedatum");
            await publishDateInput.FillAsync(publishDate.ToString("yyyy-MM-ddTHH:mm"));

            var opslaanKnop = page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
            while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
            {
                await opslaanKnop.ClickAsync();
            }

            await page.GetByRole(AriaRole.Table).WaitForAsync();
            return new(page)
            {
                IsImportant = request.IsImportant,
                Title = request.Title,
                PublishDateOffset = request.PublishDateOffset,
                PublicatieDatum = publishDate,
                PublicatieEinddatum = publishDate.AddYears(1),
                Skill = request.Skill,
                Body = request.Body,
                BerichtType = request.BerichtType,
            };
        }
      
    }

    internal class Bericht :  IAsyncDisposable
    {
        private readonly IPage _page;
        
        public Bericht(IPage page)
        {
            _page = page;
        }
        public required string Title { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime PublicatieDatum { get; set; }
        public DateTime PublicatieEinddatum { get; set; }
        public new bool IsImportant { get; set; }
        public new string? Skill { get; set; }
        public new BerichtType BerichtType { get; set; }
        public new TimeSpan? PublishDateOffset { get; set; }

        public async Task UpdateAsync(string title)
        {
            await _page.NavigateToNieuwsWerkinstructiesBeheer();
            var row = _page.GetBeheerRowByValue(title);
            await row.GetByRole(AriaRole.Link).ClickAsync();

            // Update fields
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(Title);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Rich Text Editor" }).FillAsync(Body);
            
            var belangrijk = _page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" });
            if (IsImportant)
                await belangrijk.CheckAsync();
            else
                await belangrijk.UncheckAsync();

            if (!string.IsNullOrEmpty(Skill))
            {
                var skillCheckbox = _page.GetByRole(AriaRole.Checkbox, new() { Name = Skill });
                await skillCheckbox.CheckAsync();
            }

            await _page.GetByLabel("Publicatiedatum").FillAsync(PublicatieDatum.ToString("yyyy-MM-ddTHH:mm"));
            await _page.GetByLabel("Publicatie-einddatum").FillAsync(PublicatieEinddatum.ToString("yyyy-MM-ddTHH:mm"));

            var opslaanKnop = _page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
            while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
            {
                await opslaanKnop.ClickAsync();
            }

            await _page.GetByRole(AriaRole.Table).WaitForAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _page.Context.Tracing.GroupEndAsync();
            await _page.Context.Tracing.GroupAsync("Cleanup artikel");
            await _page.NavigateToNieuwsWerkinstructiesBeheer();
            var nieuwsRows = _page.GetByRole(AriaRole.Row)
                .Filter(new()
                {
                    Has = _page.GetByRole(AriaRole.Cell, new() { Name = PublicatieDatum.ToString("dd-MM-yyyy, HH:mm"), Exact = true }).First
                });

            var deleteButton = nieuwsRows.GetByTitle("Verwijder").First;
            using (var _ = _page.AcceptAllDialogs())
            {
                await deleteButton.ClickAsync();
            }
            await _page.GetByRole(AriaRole.Table).WaitForAsync();
        }
    }

    internal enum BerichtType
    {
        Nieuws,
        Werkinstructie
    }

    internal record class CreateBerichtRequest()
    {
        public required string Title { get; init; }
        public virtual string? Body { get; init; }
        public bool IsImportant { get; init; }
        public string? Skill { get; init; }
        public BerichtType BerichtType { get; init; } = BerichtType.Nieuws;

        public TimeSpan? PublishDateOffset { get; init; }
    }
}
