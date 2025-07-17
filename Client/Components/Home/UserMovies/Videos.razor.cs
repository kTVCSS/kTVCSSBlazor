using kTVCSSBlazor.Db.Models.Home;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.UserMovies
{
    public partial class Videos
    {
        private List<Video> _videos = [];
        private bool ready = false;
        private bool isMobile = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    isMobile = await mds.IsMobileDeviceAsync();

                    _videos = await http.GetFromJsonAsync<List<Video>>("/api/uservideos");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        private async Task OnVideoClick(int id)
        {
            nm.NavigateTo($"/video/{id}");
        }
    }
}
