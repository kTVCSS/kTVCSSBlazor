using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Home;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace kTVCSSBlazor.Client.Components.Home.UserMovies
{
    public partial class UploadVideoForm
    {
        private Video _video = new Video();
        private RadzenUpload _uploader;
        private bool isUploading = false;
        int progress = 0;
        private DotNetObjectReference<UploadVideoForm>? dotNetHelper;

        void OnProgress(UploadProgressArgs args)
        {
            progress = args.Progress;

            if (progress >= 100)
            {
                isUploading = false;
            }

            InvokeAsync(StateHasChanged);
        }

        protected override void OnInitialized()
        {
            dotNetHelper = DotNetObjectReference.Create(this);
        }

        [JSInvokable]
        public async Task UpdateProgress(int value)
        {
            progress = value;
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            dotNetHelper?.Dispose();
        }

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
                try
                {
                    isUploading = true;
                    progress = 0;
                    await InvokeAsync(StateHasChanged);

                    if (file.ContentType != "video/mp4")
                    {
                        NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Только MP4 видео!");
                        await _uploader.ClearFiles();
                        return;
                    }

                    if (file.Size > 512 * 1024 * 1024)
                    {
                        NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Файл слишком большой (макс. 512 МБ)");
                        await _uploader.ClearFiles();
                        return;
                    }

                    var fileStream = file.OpenReadStream(maxAllowedSize: 512 * 1024 * 1024);
                    using var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    string uploadUrl = "http://localhost:3000/api/UploadVideo";

                    #if RELEASE 

                    uploadUrl = "https://api.ktvcss.ru/api/UploadVideo";

                    #endif

                    var result = await JSRuntime.InvokeAsync<string>(
                        "uploadWithProgress",
                        uploadUrl,
                        new File(memoryStream.ToArray(), file.Name, file.ContentType),
                        dotNetHelper);

                    var uploadResult = JsonSerializer.Deserialize<FileUploadResult>(result);
                    
                    if (uploadResult.Status)
                    {
                        _video.Url = uploadResult.Message;
                        NotificationService.Notify(NotificationSeverity.Success, "Успех", "Видео загружено!");
                    }
                    else
                    {
                        NotificationService.Notify(NotificationSeverity.Error, "Ошибка", uploadResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", ex.Message);
                }
                finally
                {
                    isUploading = false;
                    await InvokeAsync(StateHasChanged);
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

public class File
{
    public byte[] Content { get; }
    public string Name { get; }
    public string Type { get; }

    public File(byte[] content, string name, string type)
    {
        Content = content;
        Name = name;
        Type = type;
    }
}