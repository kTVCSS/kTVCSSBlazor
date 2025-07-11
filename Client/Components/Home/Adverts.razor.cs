using kTVCSS.Models.Db.Models.Common;
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
    }
}
