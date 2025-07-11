using kTVCSSBlazor.Db.Models.Highlights;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Matches
{
    public partial class MatchHighlightsPage
    {
        [Parameter]
        public int MatchID { get; set; }

        List<Result> data = [];

        bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                data = await http.GetFromJsonAsync<List<Result>>($"/api/Highlights/GetByMatch?id={MatchID}");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
