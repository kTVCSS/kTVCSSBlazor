using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Teams
{
    public partial class EditTeamProfile
    {
        [Parameter]
        public int Id { get; set; }

        private TeamPageData team = new();
        private bool ready = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    team = await http.GetFromJsonAsync<TeamPageData>($"/api/teams/getteam?id={Id}");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        private void SetDefaultAvatar()
        {
            team.AvatarUrl = $"/images/logo_ktv.png";
            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Текущая аватарка установлена на стандартную! Не забудьте сохранить изменения!");
        }

        private async Task Save()
        {
            var response = await http.PostAsJsonAsync("/api/teams/saveteam", team);

            int result = await response.Content.ReadFromJsonAsync<int>();

            if (result == 1)
            {
                NotificationService.Notify(NotificationSeverity.Success, "Успех", "Профиль команды был успешно изменен!");
                NavigationManager.NavigateTo($"/team/" + Id, false);
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Успех", "Профиль команды был успешно изменен! Однако название команды изменено не было.");
                NavigationManager.NavigateTo($"/team/" + Id, false);
            }
        }

        async Task OnClientChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                IBrowserFile uploadedFile = file;

                using var content = new MultipartFormDataContent();
                var fileStream = uploadedFile.OpenReadStream(maxAllowedSize: 10_000_000);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new(uploadedFile.ContentType);
                content.Add(streamContent, "file", uploadedFile.Name);

                var httpRequest = await http.PostAsync("/api/teams/UploadAttachment", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    team.AvatarUrl = result.Message;
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Файл успешно загружен, можете теперь публиковать пост!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", result.Message);
                }
            }
        }
    }
}
