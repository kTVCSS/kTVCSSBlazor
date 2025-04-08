using kTVCSSBlazor.Db.Models.Teams;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Teams
{
    public partial class Teams
    {
        private List<Team> dataSource = new List<Team>();
        private bool ready = false;

        public void Dispose()
        {
            dataSource = null;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    dataSource = await http.GetFromJsonAsync<List<Team>>("/api/teams/getteams");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }

            base.OnAfterRender(firstRender);
        }
    }
}
