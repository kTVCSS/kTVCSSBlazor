using kTVCSS.Models.Db.Models.Roles;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Common
{
    public partial class Contacts
    {
        List<IGrouping<string, Role>> admins = [];

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                var items = await http.GetFromJsonAsync<List<Role>>("/api/admins/getadmins");

                admins = items.OrderBy(x => x.Type).GroupBy(x => x.GroupName).ToList();

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
