using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Home;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.UserMovies
{
    public partial class UploadVideoForm
    {
        private Video _video = new Video();
        private RadzenUpload _uploader;

        async Task Submit()
        {
            if (string.IsNullOrEmpty(_video.Url))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы не загрузили видео или оно еще не успело до конца загрузиться на сервер");
                return;
            }

            if (string.IsNullOrEmpty(_video.PreviewImage))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы не загрузили картинку для превью!");
                return;
            }

            if (string.IsNullOrEmpty(_video.Title))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы не заполнили название ролика!");
                return;
            }

            _video.AuthorName = AuthProvider.CurrentUser.Username;
            _video.AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl ?? "/images/logo_ktv.png";
            _video.AuthorId = AuthProvider.CurrentUser.Id;

            var response = await http.PostAsJsonAsync("/api/uservideos/", _video);

            try
            {
                var result = await response.Content.ReadFromJsonAsync<Video>();

                if (result is not null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Видео успешно загружено, сейчас мы перенаправим Вас!");

                    await Task.Delay(1500);

                    NavigationManager.NavigateTo($"/video/{result.Id}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", ex.Message);
            }
        }

        async Task OnVideoChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                IBrowserFile uploadedFile = file;

                Console.WriteLine(file.ContentType);
                Console.WriteLine(file.Size);

                if (file.ContentType != "video/mp4")
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы должны загрузить видео, а не что-либо еще!");
                    await _uploader.ClearFiles();
                    return;
                }

                if (file.Size > 512 * 1024 * 1024)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Размер файла слишком большой");
                    await _uploader.ClearFiles();
                    return;
                }

                using var content = new MultipartFormDataContent();
                var fileStream = uploadedFile.OpenReadStream(maxAllowedSize: 512 * 1024 * 1024);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new(uploadedFile.ContentType);
                content.Add(streamContent, "file", uploadedFile.Name);

                var httpRequest = await http.PostAsync("/api/UploadVideo", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    _video.Url = result.Message;
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Видео успешно загружено! Не забудьте опубликовать ролик!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", result.Message);
                }
            }
        }

        async Task OnPreviewChange(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                IBrowserFile uploadedFile = file;

                using var content = new MultipartFormDataContent();
                var fileStream = uploadedFile.OpenReadStream(maxAllowedSize: 10_000_000);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new(uploadedFile.ContentType);
                content.Add(streamContent, "file", uploadedFile.Name);

                var httpRequest = await http.PostAsync("/api/UploadVideoPreview", content);

                var result = await httpRequest.Content.ReadFromJsonAsync<FileUploadResult>();

                if (result.Status)
                {
                    _video.PreviewImage = result.Message;
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Превьюшка успешно загружено! Не забудьте опубликовать ролик!");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", result.Message);
                }
            }
        }
    }
}
