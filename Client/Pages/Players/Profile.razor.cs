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

        public void Dispose()
        {
            Console.WriteLine("player profile disposed");
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
                Console.WriteLine("player profile is rendering");

                NavigationManager.LocationChanged += HandleLocationChanged;

                player = await http.GetFromJsonAsync<PlayerInfo>($"/api/players/getplayerbyid?id={Id}");

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
            });
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            // !!! rerender
            Dispose();
            StateHasChanged();
        }
    }
}
