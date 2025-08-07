using kTVCSSBlazor.Db.Models.Home;
using Radzen;
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

        private async Task Remove(int id)
        {
            await http.DeleteAsync($"/api/cupwinners/{id}");
            winners.RemoveAll(x => x.Id == id);
            await InvokeAsync(StateHasChanged);
            NotificationService.Notify(NotificationSeverity.Success, "Запись удалена");
        }
    }
}
