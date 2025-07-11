using kTVCSSBlazor.Db.Models.Home;
using kTVCSSBlazor.Db.Models.Players;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.Configs
{
    public partial class ConfigPage
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Id { get; set; }

        private bool ready = false;
        private GameConfig config = new();
        private PlayerInfo player = new();
        private string text = "";

        private async Task Remove()
        {
            await Http.DeleteAsync($"/api/gameconfigs/{Id}");

            ns.Notify(NotificationSeverity.Success, "Успех", "Конфиг удален!");

            await Task.Delay(1500);

            NavigationManager.NavigateTo("/#configs");
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                config = await Http.GetFromJsonAsync<GameConfig>($"api/gameconfigs/{Id}");
                player = await Http.GetFromJsonAsync<PlayerInfo>($"/api/players/getplayerbyid?id={config.AuthorId}");
                ready = true;
                await InvokeAsync(StateHasChanged);
            });
        }

        private async Task Send()
        {
            config.GameConfigsComments.Add(new()
            {
                Id = 0,
                Text = text,
                UserName = AuthProvider.CurrentUser.Username
            });

            await Http.PutAsJsonAsync($"/api/gameconfigs/{Id}", config);

            text = "";

            InvokeAsync(StateHasChanged);
        }

        void DownloadZip()
        {
            if (AuthProvider.CurrentUser is null)
            {
                ns.Notify(NotificationSeverity.Error, "VIP", "Для скачивания конфига необходимо быть авторизованным");
                return;
            }
            if (AuthProvider.CurrentUser.IsVip || config.AuthorId == AuthProvider.CurrentUser.Id)
            {
                config.Downloads += 1;
                NavigationManager.NavigateTo(config.ZipUrl, true);
                Http.PutAsJsonAsync($"/api/gameconfigs/{Id}", config);
            }
            else
            {
                ns.Notify(NotificationSeverity.Error, "VIP", "Для скачивания конфига необходимы VIP привилегии");
            }
        }
    }
}
