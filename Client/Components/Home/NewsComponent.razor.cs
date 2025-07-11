using kTVCSSBlazor.Db.Models.Arcticles;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class NewsComponent
    {
        private List<Article> News = new();
        private bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                News = await http.GetFromJsonAsync<List<Article>>("/api/articles");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
