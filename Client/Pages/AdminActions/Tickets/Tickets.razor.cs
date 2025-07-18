using kTVCSS.Models.Db.Models.Common;
using kTVCSS.Models.Db.Models.Tickets;
using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Tickets
{
    public partial class Tickets
    {
        [SupplyParameterFromQuery]
        [Parameter]
        public int? Id { get; set; }

        private bool ready = false;
        private List<Ticket> _tickets = [];
        private List<Ticket> _ticketsclosed = [];
        private bool disposed = false;

        public void Dispose()
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
            StateHasChanged();
            OnInitializedAsync();
        }

        private string ticketText = "";

        private async Task CreateTicket()
        {
            var response = await http.PostAsJsonAsync("/api/players/unbanrequest", new InitialUnbanRequest()
            {
                Message = ticketText,
                PlayerID = AuthProvider.CurrentUser.Id,
                PlayerName = AuthProvider.CurrentUser.Username
            });

            try
            {
                var ticket = await response.Content.ReadFromJsonAsync<Ticket?>();

                if (ticket is not null)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Успех", "Ваше обращение зарегистрировано! Сейчас мы Вас перенаправим в созданный тикет", TimeSpan.FromSeconds(3));

                    await Task.Delay(3000);

                    NavigationManager.NavigateTo($"/ticket/{ticket.TicketId}");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя создавать несколько обращений подряд, дождитесь рассмотрения хотя бы одной!");
                }
            }
            catch (Exception)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Ошибка", "Нельзя создавать несколько обращений подряд, дождитесь рассмотрения хотя бы одной!");
            }
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            Dispose();
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                NavigationManager.LocationChanged += HandleLocationChanged;

                Task.Run(async () =>
                {
                    int retries = 0;
                    int max = 50;

                    while (AuthProvider.CurrentUser is null)
                    {
                        if (disposed)
                        {
                            return;
                        }

                        if (retries > max)
                        {
                            ready = true;

                            await InvokeAsync(StateHasChanged);

                            return;
                        }

                        retries += 1;

                        await Task.Delay(100);
                    }

                    if (!Id.HasValue)
                    {
                        if (AuthProvider.CurrentUser.Role >= kTVCSS.Models.Db.Models.Roles.RoleType.Moderator)
                        {
                            _tickets = await http.GetFromJsonAsync<List<Ticket>>("/api/admins/gettickets");
                            _ticketsclosed = await http.GetFromJsonAsync<List<Ticket>>("/api/admins/getticketsclosed");
                        }
                    }
                    else
                    {
                        _tickets = await http.GetFromJsonAsync<List<Ticket>>("/api/players/gettickets?id=" + AuthProvider.CurrentUser.Id);
                        _ticketsclosed = await http.GetFromJsonAsync<List<Ticket>>("/api/players/getticketsclosed?id=" + AuthProvider.CurrentUser.Id);
                    }

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
