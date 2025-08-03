using kTVCSSBlazor.Db.Models.Arcticles;
using Radzen;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Pages.Library
{
    public partial class CreateArticle
    {
        private LibraryItem article = new();
        private bool ready = true;

        private async Task Add()
        {
            var response = await http.PostAsJsonAsync("/api/library", article);

            notifyService.Notify(NotificationSeverity.Success, "Успех", "Статья была создана!");

            var content = await response.Content.ReadFromJsonAsync<LibraryItem>();

            nm.NavigateTo($"/libraryrecord/{content.Id}");
        }
    }
}
