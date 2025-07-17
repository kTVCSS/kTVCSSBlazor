using kTVCSSBlazor.Db.Models.Teams;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Teams
{
    public partial class Teams
    {
        private List<Team> dataSource = new List<Team>();
        private bool ready = false;
        private int windowHeight = 0;
        private bool isMobile = false;

        public async ValueTask DisposeAsync()
        {
            WindowSize.OnResized -= (w, h) => InvokeAsync(StateHasChanged); 
            //await WindowSize.DisposeAsync();
            dataSource = null;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    isMobile = await mds.IsMobileDeviceAsync();
                    WindowSize.OnResized += (w, h) =>
                    {
                        Console.WriteLine(h);

                        if (isMobile)
                        {
                            windowHeight = h - 118;
                        }
                        else
                        {
                            windowHeight = h - 208;
                        }

                        InvokeAsync(StateHasChanged);
                    };

                    windowHeight = WindowSize.GetHeight() - (isMobile ? 118 : 208);

                    dataSource = await http.GetFromJsonAsync<List<Team>>("/api/teams/getteams");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }

            base.OnAfterRender(firstRender);
        }
    }
}
