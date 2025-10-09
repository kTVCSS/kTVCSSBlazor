using kTVCSSBlazor.Db.Models.Players;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class TotalPlayers
    {
        private List<TotalPlayer> dataSource = new List<TotalPlayer>();
        private List<TotalPlayer> _filtered = new List<TotalPlayer>();

        private int windowHeight = 0;
        private bool isMobile = false;

        private string _searchString;
        private string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;

                _filtered = dataSource.Where(QuickFilter).ToList();
            }
        }

        private bool isModerator = false;
        private bool ready = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    isMobile = await mds.IsMobileDeviceAsync();
                    WindowSize.OnResized += (w, h) =>
                    {
                        Console.WriteLine(h);

                        if (isMobile)
                        {
                            windowHeight = h - 140;
                        }
                        else
                        {
                            windowHeight = h - 258;
                        }

                        InvokeAsync(StateHasChanged);
                    };

                    windowHeight = WindowSize.GetHeight() - (isMobile ? 140 : 258);

                    dataSource = await http.GetFromJsonAsync<List<TotalPlayer>>("/api/players/gettotalplayers");

                    _filtered.AddRange(dataSource);

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        public async ValueTask DisposeAsync()
        {
            WindowSize.OnResized -= (w, h) => InvokeAsync(StateHasChanged); 
            //await WindowSize.DisposeAsync();
            dataSource = null;
            _filtered = null;
        }

        private Func<TotalPlayer, bool> QuickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.SteamID.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };
    }
}
