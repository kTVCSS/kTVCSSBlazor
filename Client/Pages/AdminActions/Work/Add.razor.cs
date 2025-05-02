using kTVCSSBlazor.Db.Models.Home;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Work
{
    public partial class Add
    {
        private WorkSuggestion work { get; set; } = new();

        private async Task OnAdd()
        {
            var responseMessage = await http.PostAsJsonAsync("/api/WorkSuggestions/", work);
            var result = await responseMessage.Content.ReadFromJsonAsync<WorkSuggestion>();

            if (result is not null)
            {
                notificationService.Notify(NotificationSeverity.Success, "Успех", "Вакансия добавлена");
                nm.NavigateTo($"/#suggestions");
            }
        }
    }
}
