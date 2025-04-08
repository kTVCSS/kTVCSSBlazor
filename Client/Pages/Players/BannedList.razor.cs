using kTVCSSBlazor.Db.Models.Players;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class BannedList
    {
        private List<BannedUser> dataSource = new List<BannedUser>();
        private List<BannedUser> _filtered = new List<BannedUser>();
        private bool ready = false;
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

        private Func<BannedUser, bool> QuickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.SteamID.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        public void Dispose()
        {
            dataSource = null;
            _filtered = null;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    dataSource = await http.GetFromJsonAsync<List<BannedUser>>("/api/players/getbannedlist");
                    _filtered.AddRange(dataSource);
                    ready = true;
                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
