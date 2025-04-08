﻿using kTVCSSBlazor.Db.Models.Teams;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Teams
{
    public partial class TeamProfile
    {
        [Parameter]
        public int Id { get; set; }

        private TeamPageData data = new();
        private IEnumerable<string> list = [];

        private string membersCardStyle = "flex-row";
        private string memberCardMargin = "width: 300px";
        private int memberCardHeight = 100;

        private bool ready = false;

        // Пример данных для карт с винрейтом
        public List<MapWinrate> MapsWinrate = [];

        // Пример данных для прогресса Total Count
        public List<MapStat> MapStats = [];

        public class MapWinrate
        {
            public string Name { get; set; }
            public string BackgroundImage { get; set; }
            public double Winrate { get; set; }
        }

        public class MapStat
        {
            public string Name { get; set; }
            public double Progress { get; set; }
        }

        private void OpenTeamEdit()
        {
            if (data.Info.BlockEdit == 1)
            {
                if (!AuthProvider.CurrentUser.IsAdmin)
                {
                    ShowError("Редактирование команды в данный момент запрещено");

                    return;
                }
            }

            NavigationManager.NavigateTo("/editteamprofile/" + Id, false);
        }

        void RowRender(RowRenderEventArgs<TeamMatch> args)
        {
            var x = args.Data;

            args.Attributes.Add("class", "row-ktv");
            args.Attributes.Add("Style", "display: grid;margin-bottom: 5px;background-image: url(/images/mapsbackgrs/" + x.MapName + ".jpg);height: 130px;background-size: 100% auto; background-position: center; background-blend-mode: multiply");
        }

        private void ShowError(string text)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Ошибка", text);
        }

        private void ShowSuccess(string text)
        {
            NotificationService.Notify(NotificationSeverity.Success, "Успех", text);
        }

        private async Task DeleteTeam()
        {
            var response = await http.PostAsJsonAsync($"/api/teams/deleteteam/{Id}", AuthProvider.CurrentUser.SteamId);

            var result = await response.Content.ReadFromJsonAsync<int>();

            if (result == 1)
            {
                ShowError("Редактирование составов на данный момент запрещено!");
                return;
            }
            if (result == 2)
            {
                ShowSuccess("Вы удалили команду!");
                NavigationManager.NavigateTo("/");
                StateHasChanged();
            }
        }

        private async Task LeaveTeam()
        {
            var response = await http.PostAsJsonAsync($"/api/teams/leaveteam", AuthProvider.CurrentUser.SteamId);

            var result = await response.Content.ReadFromJsonAsync<int>();

            if (result == 0)
            {
                ShowError("Редактирование составов на данный момент запрещено!");
                return;
            }
            if (result == 2)
            {
                ShowSuccess("Вы вышли из команды!");
                NavigationManager.NavigateTo("/myprofile");
            }
        }

        private async Task KickPlayer(string steam)
        {
            var response = await http.PostAsJsonAsync($"/api/teams/leaveteam", steam);

            var result = await response.Content.ReadFromJsonAsync<int>();

            if (result == 0)
            {
                ShowError("Редактирование составов на данный момент запрещено!");
                return;
            }
            if (result == 2)
            {
                ShowSuccess("Игрок был выгнан из команды!");
                data = await http.GetFromJsonAsync<TeamPageData>($"/api/teams/getteam?id={Id}");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    var team = await http.GetFromJsonAsync<TeamPageData>($"/api/teams/getteam?id={Id}");

                    foreach (var map in team.Matches.DistinctBy(x => x.MapName))
                    {
                        MapStats.Add(new()
                        {
                            Name = map.MapName,
                            Progress = Math.Round(Math.Round(team.Matches.Count(x => x.MapName == map.MapName) / (double)team.Matches.Count, 2) * 100, 2)
                        });

                        MapsWinrate.Add(new()
                        {
                            Name = map.MapName,
                            BackgroundImage = "/images/mapsbackgrs/" + map.MapName + ".jpg",
                            Winrate = Math.Round(Math.Round(team.Matches.Where(x => x.MapName == map.MapName && x.Victory).Count() / (double)team.Matches.Count(x => x.MapName == map.MapName), 2) * 100, 2)
                        });
                    }

                    int id = Id;

                    var captain = team.Members.FirstOrDefault(x => x.SteamID == team.Info.CapSteamID);

                    var members = team.Members.Where(x => x.SteamID != team.Info.CapSteamID).ToList();

                    data = team;

                    data.Members.Clear();

                    if (captain != null)
                    {
                        data.Members.Add(captain);
                    }

                    data.Members.AddRange(members);

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
