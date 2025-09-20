using kTVCSS.Models.Db.Models.Common;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class Adverts
    {
        private List<Advert> adverts = [];
        private bool isMobile = false;
        private bool ready = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                adverts = await http.GetFromJsonAsync<List<Advert>>($"api/guest/adverts");

                isMobile = await mds.IsMobileDeviceAsync();

                ready = true;

                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task Remove(int id)
        {
            await http.DeleteAsync($"/api/guest/removeadvert?id={id}");

            nm.Refresh(true);
        }
    }
}
