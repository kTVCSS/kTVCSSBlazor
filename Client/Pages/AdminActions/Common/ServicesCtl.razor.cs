using System;
using System.Net.Http.Json;
using kTVCSS.Models.Models;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common;

public partial class ServicesCtl
{
    private List<SystemCtlItem> services = [];
    private bool ready = false;

    protected override async Task OnInitializedAsync()
    {
        Task.Run(async () =>
        {
            services = await http.GetFromJsonAsync<List<SystemCtlItem>>("/api/admins/getservices");

            ready = true;

            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task Execute(string service, string type)
    {
        ready = false;

        await InvokeAsync(StateHasChanged);

        await http.GetAsync($"/api/admins/executeservice?service={service}&type={type}");

        services = await http.GetFromJsonAsync<List<SystemCtlItem>>("/api/admins/getservices");

        ready = true;

        await InvokeAsync(StateHasChanged);
    }
}
