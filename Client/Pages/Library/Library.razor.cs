using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using kTVCSSBlazor.Db.Models.Arcticles;

namespace kTVCSSBlazor.Client.Pages.Library;

public partial class Library
{
    private bool ready = false;

    private List<LibraryItem> items = [];
    private List<LibraryItem> searchItems = [];

    async Task OnChange(string value, string name)
    {
        Console.WriteLine(value);

        if (value.Length == 0)
        {
            searchItems = items;
        }
        else
        {
            searchItems = items.Where(x => x.Html.Contains(value, StringComparison.CurrentCultureIgnoreCase) || x.Title.Contains(value, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        Task.Run(async () =>
        {
            items = await http.GetFromJsonAsync<List<LibraryItem>>("/api/library");

            searchItems = items;

            ready = true;

            await InvokeAsync(StateHasChanged);
        });
    }
}
