using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Home;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace kTVCSSBlazor.Client.Components.Home.Configs
{
    public partial class EditConfig
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Id { get; set; }

        private GameConfig _config = new();

        protected override async Task OnInitializedAsync()
        {
            _config = await http.GetFromJsonAsync<GameConfig>($"api/gameconfigs/{Id}");
        }

        async Task Submit()
        {
            if (string.IsNullOrEmpty(_config.Name))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Вы не заполнили название конфига");
                return;
            }

            _config.AuthorName = AuthProvider.CurrentUser.Username;
            _config.AuthorAvatar = AuthProvider.CurrentUser.AvatarUrl ?? "/images/logo_ktv.png";
            _config.AuthorId = AuthProvider.CurrentUser.Id;

            var response = await http.PutAsJsonAsync("/api/gameconfigs/" + Id, _config);

            try
            {
                var result = await response.Content.ReadFromJsonAsync<GameConfig>();

                if (result is not null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Конфиг успешно изменен, сейчас мы перенаправим Вас!");

                    await Task.Delay(1500);

                    NavigationManager.NavigateTo($"/config/{result.Id}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", ex.Message);
            }
        }
    }
}
