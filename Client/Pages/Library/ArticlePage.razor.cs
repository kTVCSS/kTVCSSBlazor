using kTVCSSBlazor.Db.Models.Arcticles;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Pages.Library
{
    public partial class ArticlePage
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int Id { get; set; }

        private bool ready = false;
        private LibraryItem article;
        private string text = "";

        private async Task Remove()
        {
            await http.DeleteAsync($"/api/library/{Id}");

            notifyService.Notify(NotificationSeverity.Warning, "Успех", "Статья была удалена!");

            nm.NavigateTo("/library");
        }

        private void OpenEdit()
        {
            nm.NavigateTo($"/editlibraryitem/{Id}");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    article = await http.GetFromJsonAsync<LibraryItem>($"/api/library/{Id}");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
