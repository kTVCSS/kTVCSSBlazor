using kTVCSSBlazor.Db.Models.Home;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home.UserMovies
{
    public partial class VideoPage
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Id { get; set; }

        private bool ready = false;
        private Video video;

        private string text = "";

        private async Task Remove()
        {
            await http.DeleteAsync($"/api/uservideos/{Id}");

            NotificationService.Notify(NotificationSeverity.Success, "Успех", "Видео удалено!");

            await Task.Delay(1500);

            NavigationManager.NavigateTo("/#highlights");
        }

        private async Task Send()
        {
            video.Comments.Add(new()
            {
                Id = 0,
                Text = text,
                UserName = AuthProvider.CurrentUser.Username
            });

            await http.PutAsJsonAsync($"/api/uservideos/{Id}", video);

            text = "";

            InvokeAsync(StateHasChanged);
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                video = await http.GetFromJsonAsync<Video>($"/api/uservideos/{Id}");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
