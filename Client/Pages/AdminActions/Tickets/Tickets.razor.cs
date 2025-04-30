using kTVCSS.Models.Db.Models.Tickets;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Tickets
{
    public partial class Tickets
    {
        private bool ready = false;
        private List<Ticket> _tickets = [];

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    _tickets = await http.GetFromJsonAsync<List<Ticket>>("/api/admins/gettickets");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
