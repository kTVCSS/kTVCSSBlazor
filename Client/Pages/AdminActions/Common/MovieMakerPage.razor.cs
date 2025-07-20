using kTVCSSBlazor.Db.Models.Highlights;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common
{
    public partial class MovieMakerPage
    {
        List<Result> data = [];
        List<IGrouping<int, Result>> results = [];
        int days = 7;
        int minsecs = 20;
        bool justAces = false;
        bool justQuadros = false;
        bool isMobile = false;
        bool ready = true;

        private async Task Analyse()
        {
            Task.Run(async () =>
            {
                ready = false;

                await InvokeAsync(StateHasChanged);

                data = await http.GetFromJsonAsync<List<Result>>($"/api/Highlights/ForMovieMaker?days={days}&minsecs={minsecs}");

                if (justQuadros)
                {
                    data.RemoveAll(x => x.Type != "Quadro");
                }

                if (justAces)
                {
                    data.RemoveAll(x => x.Type != "Rampage");
                }

                results = data.GroupBy(x => x.MatchID).ToList();

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    isMobile = await mds.IsMobileDeviceAsync();

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
