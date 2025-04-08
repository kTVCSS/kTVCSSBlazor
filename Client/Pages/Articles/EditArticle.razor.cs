using kTVCSSBlazor.Db.Models.Arcticles;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Pages.Articles
{
    public partial class EditArticle
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int Id { get; set; }
        private Article article = new();
        private bool ready = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    article = await http.GetFromJsonAsync<Article>($"/api/articles/{Id}");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        private async Task Edit()
        {
            await http.PutAsJsonAsync($"/api/articles/{Id}", article);

            notifyService.Notify(NotificationSeverity.Success, "Успех", "Статья была изменена!");

            nm.NavigateTo($"/article/{article.Id}");
        }
    }
}
