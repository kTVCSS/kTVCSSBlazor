using kTVCSS.Models.Db.Models.BattleCup.DTOs;
using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http.Json;
using System.Timers;

namespace kTVCSSBlazor.Client.Pages.Matches
{
    public partial class MixRoom
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public string Guid { get; set; }

        private bool ready = false;
        private string hubUrl = string.Empty;
        private Mix mix;
        private string headerStyle = string.Empty;

        private System.Timers.Timer countdownTimer;
        private string timeLeftString;

        public double CalculateWinProbability(List<User> teamA, List<User> teamB)
        {
            foreach (var player in teamA)
            {
                if (player.CurrentMMR == 0)
                {
                    player.CurrentMMR = 1000;
                }
            }

            foreach (var player in teamB)
            {
                if (player.CurrentMMR == 0)
                {
                    player.CurrentMMR = 1000;
                }
            }

            double totalRatingA = teamA.Sum(p => p.CurrentMMR);
            double totalRatingB = teamB.Sum(p => p.CurrentMMR);

            double ratingDifference = totalRatingB - totalRatingA;
            double winProbability = 1.0 / (1.0 + Math.Pow(10, ratingDifference / 400.0));

            return Math.Round(winProbability, 2) * 100;
        }

        private string FormatTime(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
        }

        public void Dispose()
        {
            countdownTimer?.Dispose();
        }

        private void UpdateCountdown(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now < mix.DtStart)
            {
                timeLeftString = "До начала матча: " + FormatTime(mix.DtStart.Subtract(DateTime.Now));
                InvokeAsync(StateHasChanged);
            }
            else
            {
                timeLeftString = "Микс начался";
                InvokeAsync(StateHasChanged);
                countdownTimer.Stop();
                countdownTimer.Dispose();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                //UpdateTime(null, null);

#if DEBUG
                hubUrl = "http://localhost:5175/";
#endif

#if RELEASE

                hubUrl = "https://mm.ktvcss.com/";
#endif

                mix = await http.GetFromJsonAsync<Mix>(hubUrl + "api/getmixbyguid?guid=" + Guid);

                headerStyle = "background-image: url(/images/mapsbackgrs/" + mix.MapName + ".jpg);height: 300px;background-size: 200% auto;background-color: rgb(0 0 0 / 65%); background-position: center; background-blend-mode: multiply;";

                ready = true;

                await InvokeAsync(StateHasChanged);

                if (DateTime.Now > mix.DtStart)
                {
                    timeLeftString = "Микс начался";
                }
                else
                {
                    timeLeftString = "До начала матча: " + FormatTime(mix.DtStart.Subtract(DateTime.Now));
                    countdownTimer = new System.Timers.Timer(1000);
                    countdownTimer.Elapsed += UpdateCountdown;
                    countdownTimer.Start();
                }

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
