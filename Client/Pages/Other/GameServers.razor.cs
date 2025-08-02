using System;
using System.Net.Http.Json;
using kTVCSS.Models.Models;

namespace kTVCSSBlazor.Client.Pages.Other;

public partial class GameServers
{
    private List<GameServer> gameServers = [];
    private bool ready = false;

    protected override async Task OnInitializedAsync()
    {
        Task.Run(async () =>
        {
            gameServers = await http.GetFromJsonAsync<List<GameServer>>("/api/gameservers");

            ready = true;

            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task Setup(int id, GameServerType type)
    {
        await http.GetAsync($"/api/gameservers/set?id={id}&type={type}&issuier={StateProvider.CurrentUser.Username}");
        nm.Refresh(true);
    }
}
