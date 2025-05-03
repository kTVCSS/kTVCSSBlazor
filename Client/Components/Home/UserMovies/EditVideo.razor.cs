using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Highlights;
using kTVCSSBlazor.Db.Models.Home;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.UserMovies
{
    public partial class EditVideo
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Id { get; set; }

        private Video _video = new Video();

        protected override async Task OnInitializedAsync()
        {
            _video = await http.GetFromJsonAsync<Video>($"/api/uservideos/{Id}");
        }

        async Task Submit()
        {
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

            var response = await http.PutAsJsonAsync($"/api/uservideos/{_video.Id}", _video);

            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Видео успешно изменено, сейчас мы перенаправим Вас!");
            await Task.Delay(1500);

            NavigationManager.NavigateTo($"/video/{Id}");
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
