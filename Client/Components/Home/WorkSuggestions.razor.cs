using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Home;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class WorkSuggestions
    {
        private List<WorkSuggestion> workSuggestions = [];
        private bool ready = false;

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                workSuggestions = await http.GetFromJsonAsync<List<WorkSuggestion>>("/api/worksuggestions");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }


        private async Task DeleteWorkEntry(WorkSuggestion suggestion)
        {
            await http.DeleteAsync($"/api/worksuggestions/{suggestion.Id}");

            workSuggestions.Remove(suggestion);

            NotificationService.Notify(NotificationSeverity.Warning, "Успех", "Вакансия была удалена!");
        }

        private async Task MakeWorkEntry(WorkSuggestion suggestion)
        {
            await http.PostAsJsonAsync("/api/MakeEntry2Work",
                new WorkEntry()
                {
                    UserName = AuthProvider.CurrentUser.Username,
                    WorkSuggestion = suggestion
                });

            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Ваше желание помочь проекту было доставлено администрации, спасибо!");
        }
    }
}
