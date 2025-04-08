using kTVCSSBlazor.Db.Models.Arcticles;
using Radzen;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Pages.Articles
{
    public partial class CreateArticle
    {
        private Article article = new();
        private bool ready = true;

        private async Task Add()
        {
            var response = await http.PostAsJsonAsync("/api/articles", article);

            notifyService.Notify(NotificationSeverity.Success, "Успех", "Статья была создана!");

            var content = await response.Content.ReadFromJsonAsync<Article>();

            nm.NavigateTo($"/article/{content.Id}");
        }
    }
}
