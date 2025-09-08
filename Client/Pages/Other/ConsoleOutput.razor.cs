using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Other
{
    public partial class ConsoleOutput
    {
        private string output = "";
        private string cmd = "";
        private bool disabled = false;
        private DotNetObjectReference<ConsoleOutput>? objRef;
        private ElementReference window;
        private bool access = false;
        private int windowHeight = 0;
        private bool isMobile = false;

        private string baseUri = "http://localhost:3000";

        public enum ActionType
        {
            Start,
            Stop,
            Restart
        }

        private async Task Execute(ActionType type)
        {
            disabled = true;

            InvokeAsync(StateHasChanged);

            await http.GetAsync(baseUri + "/api/gameservermanager/" + type.ToString());

            if (type == ActionType.Restart || type == ActionType.Start)
            {
                Nav.Refresh(true);
            }
            else
            {
                output += "\r\nСЕРВЕР ОСТАНОВЛЕН";
                InvokeAsync(StateHasChanged);
                await ScrollToBottom();
            }

            disabled = false;

            InvokeAsync(StateHasChanged);
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return;

            var request = new RconQuery
            {
                Query = cmd,
                Address = "46.174.54.94",
                Password = "Zxc1337Zxc",
                Port = 27018
            };

            try
            {
                await http.PostAsJsonAsync(baseUri + "/api/rcon", request);
                cmd = string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
        }

        protected override Task OnInitializedAsync()
        {
#if DEBUG

            //Console.WriteLine("asd");

#endif

#if RELEASE

        baseUri = "https://api.ktvcss.com";

#endif

            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                while (AuthProvider.CurrentUser is null)
                {
                    await Task.Delay(100);
                }

                if (AuthProvider.CurrentUser.Username == "kurwanator1337" || AuthProvider.CurrentUser.Username == "phenom" || AuthProvider.CurrentUser.Username == "raxzerax")
                {
                    access = true;
                    objRef = DotNetObjectReference.Create(this);
                    string wsUrl = string.Empty;
                    if (baseUri.StartsWith("https"))
                    {
                        wsUrl = baseUri.Replace("https://", "wss://").TrimEnd('/') + "/ws/console";
                    }
                    else
                    {
                        wsUrl = baseUri.Replace("http://", "ws://").TrimEnd('/') + "/ws/console";
                    }
                    await JS.InvokeVoidAsync("connectConsole", objRef, wsUrl);

                    isMobile = await mds.IsMobileDeviceAsync();
                    WindowSize.OnResized += (w, h) =>
                    {
                        Console.WriteLine(h);

                        if (isMobile)
                        {
                            windowHeight = h - 256;
                        }
                        else
                        {
                            windowHeight = h - 324;
                        }

                        InvokeAsync(StateHasChanged);
                    };

                    windowHeight = WindowSize.GetHeight() - (isMobile ? 256 : 324);

                    InvokeAsync(StateHasChanged);
                }
            }
        }

        [JSInvokable]
        public async Task ReceiveConsoleMessage(string message)
        {
            output += message;
            InvokeAsync(StateHasChanged);
            await ScrollToBottom();
        }

        [JSInvokable]
        public async Task WebSocketClosed()
        {
            output += "\nWebSocket closed.";
            InvokeAsync(StateHasChanged);
            await ScrollToBottom();
        }

        public async ValueTask DisposeAsync()
        {
            WindowSize.OnResized -= (w, h) => InvokeAsync(StateHasChanged); 
            //await WindowSize.DisposeAsync();
            objRef?.Dispose();
        }

        private async Task ScrollToBottom()
        {
            await Task.Delay(100); // Небольшая задержка для рендеринга
            try
            {
                await JS.InvokeVoidAsync("scrollToBottom", window);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling to bottom: {ex.Message}");
            }
        }
    }
}
