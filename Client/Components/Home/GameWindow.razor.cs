using System;
using System.Net.Http.Json;
using kTVCSS.Models.Models;
using kTVCSSBlazor.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Radzen;

namespace kTVCSSBlazor.Client.Components.Home;

public partial class GameWindow
{
    [Parameter]
    public bool InSearch { get; set; } = false;

    [Parameter]
    public EventCallback<bool> InSearchChanged { get; set; }

    [Parameter]
    public bool SearchButtonEnabled { get; set; } = true;

    [Parameter]
    public EventCallback<bool> SearchButtonEnabledChanged { get; set; }

    [Parameter]
    public int CurrentSearchUsersCount { get; set; } = 0;

    [Parameter]
    public EventCallback<int> CurrentSearchUsersCountChanged { get; set; }

    [Parameter]
    public MMService Hub { get; set; }

    private bool disposed = false;

    public void Dispose()
    {
        disposed = true;
    }

    private async Task UpdateInSearch(bool newValue)
    {
        InSearch = newValue;
        await InSearchChanged.InvokeAsync(InSearch);
    }

    private async Task UpdateSearchButtonEnabled(bool newValue)
    {
        SearchButtonEnabled = newValue;
        await SearchButtonEnabledChanged.InvokeAsync(SearchButtonEnabled);
    }

    private async Task UpdateCurrentSearchUsersCount(int newValue)
    {
        CurrentSearchUsersCount = newValue;
        await CurrentSearchUsersCountChanged.InvokeAsync(CurrentSearchUsersCount);

        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        Hub.Connection.On<GameHubConnectResult>("GetConnectResult", async (result) =>
        {
            Console.WriteLine(result.ToString());

            switch (result)
            {
                case GameHubConnectResult.Ok:
                    {
                        UpdateInSearch(true);
                        break;
                    }
                case GameHubConnectResult.NoPlayerData:
                    {
                        UpdateInSearch(false);
                        break;
                    }
                case GameHubConnectResult.AlreadyExists:
                    {
                        UpdateInSearch(false);
                        break;
                    }
                case GameHubConnectResult.AlreadyPlaying:
                    {
                        UpdateInSearch(false);
                        break;
                    }
                case GameHubConnectResult.IsBanned:
                    {
                        UpdateInSearch(false);
                        break;
                    }
                case GameHubConnectResult.MixesDisabled:
                    {
                        UpdateInSearch(false);
                        break;
                    }
            }
        });

        string hubUrl = "";

#if DEBUG
        hubUrl = "http://localhost:5175/";
#endif

#if RELEASE
        hubUrl = "https://mm.ktvcss.ru/";
#endif

        Task.Run(async () =>
        {
            while (!disposed)
            {
                if (disposed) return;

                int count = await http.GetFromJsonAsync<int>(hubUrl + "api/getplayerscount");

                UpdateCurrentSearchUsersCount(count);

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        });
    }

    public async Task Start()
    {
        UpdateSearchButtonEnabled(false);

        await InvokeAsync(StateHasChanged);

        try
        {
            await Hub.StartAsync();
            await Hub.OnAfterConnect(AuthProvider.CurrentUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            ns.Notify(NotificationSeverity.Error, "Не удалось войти в поиск. Подробности ошибки в консоли браузера (F12).");
        }

        Task.Run(async () =>
        {
            await Task.Delay(2000);

            UpdateSearchButtonEnabled(true);

            await InvokeAsync(StateHasChanged);
        });
    }

    public async Task Stop()
    {
        UpdateSearchButtonEnabled(false);

        await InvokeAsync(StateHasChanged);

        await Hub.Connection.StopAsync();

        UpdateInSearch(false);

        await InvokeAsync(StateHasChanged);

        Task.Run(async () =>
        {
            await Task.Delay(2000);

            UpdateSearchButtonEnabled(true);

            await InvokeAsync(StateHasChanged);
        });
    }
}
