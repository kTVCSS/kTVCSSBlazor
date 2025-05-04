using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Home;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;
using System;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.Configs
{
    public partial class UploadConfigForm
    {
        private GameConfig _config = new();
        private string guid = Guid.NewGuid().ToString();
        private bool canPost = false;
        private string _uploadPath;

        protected override Task OnInitializedAsync()
        {
            _uploadPath = http.BaseAddress + "api/uploadconfigfiles?guid=" + guid;

            return base.OnInitializedAsync();
        }

        async Task Submit()
        {
            if (!canPost)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы не загрузили файлы или они еще не успели до конца загрузиться на сервер");
                return;
            }

            if (string.IsNullOrEmpty(_config.Name))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы не заполнили название конфига");
                return;
            }

            _config.AuthorName = AuthProvider.CurrentUser.Username;
            _config.AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl ?? "/images/logo_ktv.png";
            _config.AuthorId = AuthProvider.CurrentUser.Id;

            var response = await http.PostAsJsonAsync("/api/gameconfigs/", _config);

            try
            {
                var result = await response.Content.ReadFromJsonAsync<GameConfig>();

                if (result is not null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Конфиг успешно загружен, сейчас мы перенаправим Вас!");

                    await Task.Delay(1500);

                    NavigationManager.NavigateTo($"/config/{result.Id}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", ex.Message);
            }
        }

        void OnProgress(UploadProgressArgs args)
        {
            if (args.Progress == 100)
            {
                canPost = true;
                _config.ZipUrl = $"/configs/{guid}.zip";
                NotificationService.Notify(NotificationSeverity.Info, "Успех", "Файлы были успешно загружены на сервер!");
            }
        }
    }
}
