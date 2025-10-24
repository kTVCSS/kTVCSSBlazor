using kTVCSS.Models.Db.Models.Statuses;
using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class EditProfile
    {
        [Parameter]
        public int Id { get; set; }

        private kTVCSSBlazor.Db.Models.Players.Profile profile = new();
        private bool ready = false;

        private bool needUpdatePassword = false;
        private string originalLogin = string.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    profile = await http.GetFromJsonAsync<kTVCSSBlazor.Db.Models.Players.Profile>($"/api/players/getplayerprofile?id={Id}");

                    originalLogin = profile.Login;

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

        public bool IsValidLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
                return false;

            if (login.Length > 30)
                return false;

            // Регулярное выражение: от 1 до 30 символов, каждый из которых – буква, цифра, '_' или '-'
            const string pattern = @"^[A-Za-z0-9_-]+$";
            return Regex.IsMatch(login, pattern);
        }

        private async Task Save()
        {
            if (needUpdatePassword)
            {
                if (!pswd.ValidatePassword(profile.Password))
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Чтобы изменить пароль Ваш пароль должен содержать хотя бы один спецсимвол, заглавную букву и цифру");
                    return;
                }
            }

            if (profile.Login.Length < 4)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Логин должен быть длиннее 3 символов");
                return;
            }

            if (!IsValidLogin(profile.Login))
            {
                NotificationService.Notify(new NotificationMessage()
                {
                    CloseOnClick = true,
                    Detail = "Логин содержит недопустимые знаки!",
                    Summary = "Ошибка регистрации",
                    Severity = NotificationSeverity.Error,
                    Duration = 5000,
                    Payload = DateTime.Now
                });

                return;
            }

            if (string.IsNullOrEmpty(originalLogin))
            {
                originalLogin = "debug";
            }

            var response = await http.PostAsJsonAsync($"/api/players/saveprofile?id={Id}&updatePassword={needUpdatePassword}&newLogin={!originalLogin.Equals(profile.Login)}", profile);

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
            else if (result == SaveProfileResult.LoginExists)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Такой логин уже используется на проекте");
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
