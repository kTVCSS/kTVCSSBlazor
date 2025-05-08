using kTVCSS.Models.Db.Models.Statuses;
using kTVCSSBlazor.Db.Models.Players;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class Profile
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int Id { get; set; }

        private PlayerInfo player;

        private bool ready = false;

        private bool isOnline = false;

        private bool isMeAFriend = false;

        private kTVCSS.Models.Db.Models.Players.FriendRequest? FriendRequest;

        public List<MapWinrate> MapsWinrate = [];

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

        public void Dispose()
        {
            ready = false;
            player = null;
            NavigationManager.LocationChanged -= HandleLocationChanged;
            StateHasChanged();
            OnInitializedAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                NavigationManager.LocationChanged += HandleLocationChanged;

                player = await http.GetFromJsonAsync<PlayerInfo>($"/api/players/getplayerbyid?id={Id}");

                isMeAFriend = await http.GetFromJsonAsync<bool>($"/api/friendsengine/ismefriend?playerid={AuthProvider.CurrentUser.Id}&friendid={Id}");

                try
                {
                    var tst = await http.GetFromJsonAsync<kTVCSS.Models.Db.Models.Players.FriendRequest?>($"/api/friendsengine/GetFriendRequest?requesterId={AuthProvider.CurrentUser.Id}&addresseeId={Id}");

                    if (tst is not null)
                    {
                        FriendRequest = tst;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

                foreach (var map in player.LastTwentyMatches.DistinctBy(x => x.MapName))
                {
                    MapStats.Add(new()
                    {
                        Name = map.MapName,
                        Progress = Math.Round(Math.Round(player.LastTwentyMatches.Count(x => x.MapName == map.MapName) / (double)player.LastTwentyMatches.Count, 2) * 100, 2)
                    });

                    MapsWinrate.Add(new()
                    {
                        Name = map.MapName,
                        BackgroundImage = "/images/mapsbackgrs/" + map.MapName + ".jpg",
                        Winrate = Math.Round(Math.Round(player.LastTwentyMatches.Where(x => x.MapName == map.MapName && x.Victory).Count() / (double)player.LastTwentyMatches.Count(x => x.MapName == map.MapName), 2) * 100, 2)
                    });
                }

                ready = true;

                await InvokeAsync(StateHasChanged);

                if (FriendRequest is not null)
                {
                    if (FriendRequest.Requester.PlayerID != AuthProvider.CurrentUser.Id)
                    {
                        NotificationService.Notify(Radzen.NotificationSeverity.Info, "Заявка в друзья", "Вас хотят добавить в друзья! Принять или отклонить заявку можно в меню действий под шапкой игрока.");
                    }
                }
            });
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            Dispose();
            StateHasChanged();
        }
    }
}
