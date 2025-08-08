using kTVCSS.Models.Db.Models.Common;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class Adverts
    {
        private List<Advert> adverts = [];

        protected override async Task OnInitializedAsync()
        {
            adverts = await http.GetFromJsonAsync<List<Advert>>($"api/guest/adverts");
        }

        private async Task Remove(int id)
        {
            await http.DeleteAsync($"/api/guest/removeadvert?id={id}");

            nm.Refresh(true);
        }
    }
}
