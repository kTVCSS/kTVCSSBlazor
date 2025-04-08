using kTVCSSBlazor.Db.Models.Arcticles;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Pages.Articles
{
    public partial class ArticlePage
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int Id { get; set; }

        private bool ready = false;
        private Article article;
        private string text = "";

        private async Task Remove()
        {
            await http.DeleteAsync($"/api/articles/{Id}");

            notifyService.Notify(NotificationSeverity.Warning, "Успех", "Новость была удалена!");

            nm.NavigateTo("/");
        }

        private void OpenEdit()
        {
            nm.NavigateTo($"/editarticle/{Id}");
        }

        private async Task Send()
        {
            article.ArticleComments.Add(new()
            {
                Text = text,
                UserName = AuthProvider.CurrentUser.Username
            });

            await http.PutAsJsonAsync($"/api/articles/{Id}", article);

            article = await http.GetFromJsonAsync<Article>($"/api/articles?id={Id}");

            text = "";

            InvokeAsync(StateHasChanged);
        }

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
    }
}
