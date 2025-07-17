using kTVCSS.Models.Db.Models.BattleCup;
using kTVCSS.Models.Db.Models.BattleCup.DTOs;
using kTVCSS.Models.Db.Models.BattleCup.Enums;
using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Timers;

namespace kTVCSSBlazor.Client.Pages.Cups
{
    public partial class BattleCups
    {
        private CupDto cup;
        private List<GameServer> gameServers = [];

        private bool ready = false;

        private System.Timers.Timer? timer;
        private TimeSpan timeLeft;
        private string formattedTime = "";

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                cup = await http.GetFromJsonAsync<CupDto>("/api/battlecup");
                gameServers = await http.GetFromJsonAsync<List<GameServer>>("/api/gameservers");

                if (cup.Status == CupStatus.OpenForRegistration)
                {
                    CalculateTimeLeft();
                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += OnTimerElapsed;
                    timer.Start();
                }

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }

        private async Task Participate()
        {
            await http.GetAsync("/api/battlecup/participate?id=" + AuthProvider.CurrentUser.TeamID);
            NavigationManager.Refresh(true);
        }

        private async Task RemoveEntry()
        {
            await http.GetAsync("/api/battlecup/removeentry?id=" + AuthProvider.CurrentUser.TeamID);
            NavigationManager.Refresh(true);
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            CalculateTimeLeft();
            InvokeAsync(StateHasChanged);
        }

        private void CalculateTimeLeft()
        {
            TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, moscowTimeZone);
            DateTime target = now.Date.AddHours(18);

            if (now > target)
                target = target.AddDays(1);

            timeLeft = target - now;
            formattedTime = $"{timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
        }
    }
}
