﻿using kTVCSS.Models.Models;
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
        private bool unbanning = false;
        private int windowHeight = 0;
        private bool isMobile = false;

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

        private async Task Unban()
        {
            unbanning = true;

            await InvokeAsync(StateHasChanged);

            var players = _filtered.Where(x => x.BannedBy == "Система");

            foreach (var player in players)
            {
                var ban = new Ban()
                {
                    AdminName = AuthProvider.CurrentUser.Username,
                    DaysAdder = 0,
                    Reason = "Снятие банов системы",
                    Target = player.Id
                };

                await http.PostAsJsonAsync("/api/admins/unban", ban);
            }

            NavigationManager.Refresh(true);
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
                    isMobile = await mds.IsMobileDeviceAsync();
                    WindowSize.OnResized += (w, h) =>
                    {
                        Console.WriteLine(h);

                        if (isMobile)
                        {
                            windowHeight = h - 118;
                        }
                        else
                        {
                            windowHeight = h - 208;
                        }

                        InvokeAsync(StateHasChanged);
                    };

                    windowHeight = WindowSize.GetHeight() - (isMobile ? 118 : 208);
                    dataSource = await http.GetFromJsonAsync<List<BannedUser>>("/api/players/getbannedlist");
                    _filtered.AddRange(dataSource);
                    ready = true;
                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
