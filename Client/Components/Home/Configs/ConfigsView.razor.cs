using kTVCSSBlazor.Db.Models.Home;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.Configs
{
    public partial class ConfigsView
    {
        List<GameConfig> _configs = [];
        bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                _configs = await http.GetFromJsonAsync<List<GameConfig>>("/api/gameconfigs");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
