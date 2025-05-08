using System.Net.Http.Json;
using System.Net.NetworkInformation;
using kTVCSS.Models.Models;
using Radzen;
using Radzen.Blazor;

namespace kTVCSSBlazor.Client.Pages.FAQ
{
    public partial class FAQ
    {
        private List<FaqItem> items;
        private bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                items = await http.GetFromJsonAsync<List<FaqItem>>("/api/faq");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
