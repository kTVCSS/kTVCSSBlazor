﻿using kTVCSS.Models.Db.Models.Tickets;
using kTVCSSBlazor.Db.Models.IM;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Tickets
{
    public partial class TicketPage
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int Id { get; set; }

        private bool ready = false;

        private Ticket _ticket = new();

        private bool isMobile = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                isMobile = await mds.IsMobileDeviceAsync();

                Task.Run(async () =>
                {
                    _ticket = await http.GetFromJsonAsync<Ticket>("/api/players/getticket?id=" + Id);

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        private string NewMessage { get; set; } = string.Empty;

        private void SendMessage()
        {
            var text = NewMessage?.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            DateTime now = DateTime.UtcNow.AddHours(3);

            var msg = new TicketMessage
            {
                TicketId = Id,
                Ticket = _ticket,
                MessageText = text,
                CreatedAt = now,
                SenderType = AuthProvider.CurrentUser.Role >= kTVCSS.Models.Db.Models.Roles.RoleType.Moderator ? SenderType.Admin : SenderType.Player,
                UserName = AuthProvider.CurrentUser.Username
            };

            _ticket.Messages.Add(msg);

            // не ждем результат
            http.PostAsJsonAsync("/api/players/sendticketmessage", msg);

            NewMessage = string.Empty;

            StateHasChanged();
        }

        private async Task Close()
        {
            await http.GetAsync($"/api/players/closeticket?id={Id}");

            NavigationManager.Refresh(true);
        }
    }
}
