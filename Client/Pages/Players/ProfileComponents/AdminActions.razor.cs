using kTVCSS.Models.Db.Models.Common;
using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Players;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players.ProfileComponents
{
    public partial class AdminActions
    {
        [Parameter] public PlayerInfo player { get; set; }
        [Parameter] public bool isOnline { get; set; }

        void ShowTooltip(ElementReference elementReference, string text, TooltipOptions options = null) => tooltipService.Open(elementReference, text, options);

        private async Task ExecuteSetMMR()
        {
            await http.GetAsync($"/api/players/setmmr?id={player.MainInfo.Id}&mmr={player.MainInfo.MMR}&admin={AuthProvider.CurrentUser.Username}");

            ns.Notify(NotificationSeverity.Success, "Успех", "Игроку задан новый рейтинг!");

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private string unbanReason;

        private async Task UnbanPlayer()
        {
            Ban ban = new Ban();
            ban.AdminName = AuthProvider.CurrentUser.Username;
            ban.Target = player.MainInfo.Id;
            ban.DaysAdder = 0;
            ban.Reason = unbanReason;

            await http.PostAsJsonAsync("/api/admins/unban", ban);

            ns.Notify(NotificationSeverity.Success, "Успех", "Игрок был разблокирован!");

            await Task.Delay(1000);

            nm.Refresh(true);
        }

        private BanReason _selectedReason;

        private async Task Ban()
        {
            Ban ban = new Ban();
            ban.AdminName = AuthProvider.CurrentUser.Username;
            ban.Target = player.MainInfo.Id;
            ban.DaysAdder = _selectedReason.DaysAdder;
            ban.Reason = _selectedReason.Name;

            await http.PostAsJsonAsync("/api/admins/ban", ban);

            ns.Notify(NotificationSeverity.Success, "Успех", "Игрок был заблокирован!");

            await Task.Delay(1000);

            nm.Refresh(true);
        }
    }
}
