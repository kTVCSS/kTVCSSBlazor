using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Other
{
    public partial class Dashboard
    {
        private List<kTVCSS.Models.Models.Dashboard> data = [];
        private bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                data = await http.GetFromJsonAsync<List<kTVCSS.Models.Models.Dashboard>>("/api/admins/getdashboardinfo");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
