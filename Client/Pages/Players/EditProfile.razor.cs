using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using kTVCSS.Models.Db.Models.Statuses;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class EditProfile
    {
        [Parameter]
        public int Id { get; set; }

        private kTVCSSBlazor.Db.Models.Players.Profile profile = new();
        private bool ready = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    profile = await http.GetFromJsonAsync<kTVCSSBlazor.Db.Models.Players.Profile>($"/api/players/getplayerprofile?id={Id}");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        private void SetDefaultAvatar()
        {
            profile.AvatarUrl = $"/images/logo_ktv.png";
            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Текущая аватарка установлена на стандартную! Не забудьте сохранить изменения!");
        }

        private void SetDefaultHeader()
        {
            profile.HeaderUrl = $"";
            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Текущая шапка установлена на стандартную! Не забудьте сохранить изменения!");
        }

        private async Task Save()
        {
            var response = await http.PostAsJsonAsync($"/api/players/saveprofile?id={Id}", profile);

            SaveProfileResult result = await response.Content.ReadFromJsonAsync<SaveProfileResult>();

            if (result == SaveProfileResult.Ok)
            {
                NotificationService.Notify(NotificationSeverity.Success, "Успех", "Профиль был успешно изменен!");
                await Task.Delay(1000);
                NavigationManager.NavigateTo($"/player/" + Id, true);
            }
            else if (result == SaveProfileResult.TelegramAlreadyUsing)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Этот телеграм уже кем-то используется");
            }
            else if (result == SaveProfileResult.BadTelegram)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "При привязке телеграма нужно использовать не ник, а ID - посмотрите внимательнее, пожалуйста");
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

                var httpRequest = await http.PostAsync("/api/players/uploadavatar", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    profile.AvatarUrl = result.Message;
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Файл успешно загружен, не забудьте сохранить профиль!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", result.Message);
                }
            }
        }

        async Task OnClientChangeHeader(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                IBrowserFile uploadedFile = file;

                using var content = new MultipartFormDataContent();
                var fileStream = uploadedFile.OpenReadStream(maxAllowedSize: 10_000_000);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new(uploadedFile.ContentType);
                content.Add(streamContent, "file", uploadedFile.Name);

                var httpRequest = await http.PostAsync("/api/players/uploadheader", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    profile.HeaderUrl = result.Message;
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Файл успешно загружен, не забудьте сохранить профиль!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", result.Message);
                }
            }
        }
    }
}
