using System.Net.Http.Json;
using System.Net.NetworkInformation;
using kTVCSS.Models.Models;
using Radzen;
using Radzen.Blazor;

namespace kTVCSSBlazor.Client.Pages.Common
{
    public partial class FAQ
    {
        private List<FaqItem> items;
        private bool ready = false;

        private async Task Delete(FaqItem item)
        {
            await http.DeleteAsync("/api/faq/" + item.Id);

            items = await http.GetFromJsonAsync<List<FaqItem>>("/api/faq");

            await InvokeAsync(StateHasChanged);
        }

        private async Task Add(string question, string answer)
        {
            var faq = new FaqItem()
            {
                Question = question,
                Answer = answer
            };

            await http.PostAsJsonAsync("/api/faq", faq);

            await OnInitializedAsync();

            await InvokeAsync(StateHasChanged);
        }

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
