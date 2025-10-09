using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace kTVCSSBlazor.Client.Services
{
    public class ChatHubService : IAsyncDisposable
    {
        public HubConnection Connection { get; private set; }
        private Uri _url { get; set; }

        public ChatHubService(NavigationManager navManager)
        {
#if DEBUG
            _url = navManager.ToAbsoluteUri("http://localhost:4050/chathub");
#endif
#if RELEASE
            _url = navManager.ToAbsoluteUri("https://chat.ktvcss.com/chathub");
#endif

            Connection = new HubConnectionBuilder()
                .WithUrl(_url)
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task StartAsync()
        {
            if (Connection.State != HubConnectionState.Connected)
            {
                try
                {
                    await Connection.StopAsync();

                    await Connection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }

                Connection = new HubConnectionBuilder()
                    .WithUrl(_url)
                    .WithAutomaticReconnect()
                    .Build();

                await Connection.StartAsync();
            } 
        }

        public async Task OnAfterConnect(int id)
        {
            await StartAsync();

            await Connection.InvokeAsync("OnAfterConnectAsync", id);
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }
    }
}
