using kTVCSSBlazor.Db.Models.Arcticles;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Pages.Library
{
    public partial class EditArticle
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int Id { get; set; }
        private LibraryItem article = new();
        private bool ready = false;

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

        private async Task Edit()
        {
            await http.PutAsJsonAsync($"/api/Library/{Id}", article);

            notifyService.Notify(NotificationSeverity.Success, "Успех", "Статья была изменена!");

            nm.NavigateTo($"/libraryrecord/{article.Id}");
        }
    }
}
