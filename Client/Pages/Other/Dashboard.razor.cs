using kTVCSS.Models.Db.Models.Common;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Other
{
    public partial class Dashboard
    {
        private List<kTVCSS.Models.Models.Dashboard> data = [];
        private List<kTVCSS.Models.Models.DashboardAdminLog> admins = [];
        private List<NLogRecord> logs = [];
        private bool ready = false;
        private int days = 7;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                data = await http.GetFromJsonAsync<List<kTVCSS.Models.Models.Dashboard>>("/api/admins/getdashboardinfo?days=" + days);

                admins = await http.GetFromJsonAsync<List<kTVCSS.Models.Models.DashboardAdminLog>>("/api/admins/GetDashboardInfoAdmins?days=" + days);
                
                logs = await http.GetFromJsonAsync<List<NLogRecord>>("/api/admins/GetLogs?days=" + days);

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }

        private async Task Apply()
        {
            ready = false;

            await InvokeAsync(StateHasChanged);

            data = await http.GetFromJsonAsync<List<kTVCSS.Models.Models.Dashboard>>("/api/admins/getdashboardinfo?days=" + days);

            admins = await http.GetFromJsonAsync<List<kTVCSS.Models.Models.DashboardAdminLog>>("/api/admins/GetDashboardInfoAdmins?days=" + days);

            logs = await http.GetFromJsonAsync<List<NLogRecord>>("/api/admins/GetLogs?days=" + days);

            ready = true;

            await InvokeAsync(StateHasChanged);
        }
    }
}
