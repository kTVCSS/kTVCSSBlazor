using kTVCSS.Models.Db.Models.Home;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class NotAuthorizedView
    {
        private GuestStat stat = new();
        private bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                stat = await http.GetFromJsonAsync<GuestStat>("/api/guest");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
