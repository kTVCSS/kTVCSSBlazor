using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Highlights;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common
{
    public partial class MovieMakerPage
    {
        List<Result> data = [];
        List<IGrouping<int, Result>> results = [];
        DateTime dtFrom = DateTime.Now;
        DateTime dtTo = DateTime.Now;
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

                var from = dtFrom.ToString("yyyy-MM-dd HH:mm:ss");
                var to = dtTo.ToString("yyyy-MM-dd HH:mm:ss");

                MMRequest request = new MMRequest()
                {
                    DTFrom = DateTime.Parse(from),
                    DTTo = DateTime.Parse(to),
                    MinSeconds = minsecs
                };

                var response = await http.PostAsJsonAsync($"/api/Highlights/ForMovieMaker", request);

                data = await response.Content.ReadFromJsonAsync<List<Result>>();

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
                    dtFrom = dtFrom.AddDays(-7);

                    isMobile = await mds.IsMobileDeviceAsync();

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
