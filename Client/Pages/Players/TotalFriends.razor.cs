using kTVCSSBlazor.Db.Models.Players;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class TotalFriends
    {
        private List<TotalPlayer> dataSource = new List<TotalPlayer>();
        private List<TotalPlayer> _filtered = new List<TotalPlayer>();
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
        private bool isMobile = false;
        private bool isModerator = false;
        private bool ready = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    while (kTVCSSAuthenticationStateProvider.CurrentUser is null)
                    {
                        await Task.Delay(1000);
                    }

                    isMobile = await mds.IsMobileDeviceAsync();

                    dataSource = await http.GetFromJsonAsync<List<TotalPlayer>>($"/api/friendsengine/getfriends?playerid={kTVCSSAuthenticationStateProvider.CurrentUser.Id}");

                    _filtered.AddRange(dataSource);

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        public void Dispose()
        {
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
