using kTVCSSBlazor.Db.Models.Home;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class CupWinners
    {
        private List<CupWinner> winners = [];

        protected override async Task OnInitializedAsync()
        {
            winners = await http.GetFromJsonAsync<List<CupWinner>>("/api/cupwinners");
        }
    }
}
