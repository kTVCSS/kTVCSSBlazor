using kTVCSSBlazor.Db.Models.Highlights;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class PlayerHighlightsPage
    {
        [Parameter]
        public int PlayerID { get; set; }

        List<Result> data = [];
        List<IGrouping<int, Result>> results = [];

        bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                data = await http.GetFromJsonAsync<List<Result>>($"/api/Highlights/GetByPlayer?id={PlayerID}");

                results = data.GroupBy(x => x.MatchID).ToList();

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
