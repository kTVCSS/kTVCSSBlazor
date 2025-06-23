using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace kTVCSSBlazor.Client.Services
{
    public class ChatHubService : IAsyncDisposable
    {
        public HubConnection Connection { get; private set; }

        public ChatHubService(NavigationManager navManager)
        {
#if DEBUG
            Connection = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("http://localhost:4050/chathub"))
                .WithAutomaticReconnect()
                .Build();
#endif
#if RELEASE

            Connection = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("https://chat.ktvcss.ru/chathub"))
                .WithAutomaticReconnect()
                .Build();
#endif
        }

        public async Task StartAsync()
        {
            if (Connection.State == HubConnectionState.Disconnected)
                await Connection.StartAsync();
        }

        public async Task OnAfterConnect(int id)
        {
            if (Connection.State == HubConnectionState.Disconnected)
                await Connection.StartAsync();

            await Connection.InvokeAsync("OnAfterConnectAsync", id);
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }
    }
}
