using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace kTVCSSBlazor.Client.Services
{
    public class MMService : IAsyncDisposable
    {
        public HubConnection Connection { get; private set; }

        public MMService(NavigationManager navManager)
        {
#if DEBUG
            Connection = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("http://localhost:5175/gamehub"))
                .WithAutomaticReconnect()
                .Build();
#endif
#if RELEASE

            Connection = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("https://mm.ktvcss.ru/gamehub"))
                .WithAutomaticReconnect()
                .Build();
#endif
        }

        public async Task StartAsync()
        {
            if (Connection.State == HubConnectionState.Disconnected)
                await Connection.StartAsync();
        }

        public async Task OnAfterConnect(User player)
        {
            if (Connection.State == HubConnectionState.Disconnected)
                await Connection.StartAsync();

            await Connection.InvokeAsync("OnAfterConnectAsync", player);
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }
    }
}
