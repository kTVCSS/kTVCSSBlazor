using kTVCSS.Models.Db.Models.Common;
using Microsoft.AspNetCore.Components.Routing;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components
{
    public partial class NotificationBell
    {
        private bool drawerVisible;
        private int unreadCount;
        private List<Notification> notifications;
        private bool _disposed = false;
        private bool firstLoad = true;

        public void Dispose()
        {
            _disposed = true;
            nm.LocationChanged -= OnLocationChanged;
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            Task.Run(async () =>
            {
                // фича, чтобы не задрачивать бэк
                int random = new Random().Next(0, 3);

                if (random == 2)
                {
                    LoadNotifications();
                }
            });
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                while (!_disposed)
                {
                    await LoadNotifications();

                    if (firstLoad)
                    {
                        firstLoad = false;
                        if (unreadCount > 0)
                        {
                            string notifyText = "";

                            if (unreadCount == 1)
                            {
                                notifyText = "У Вас 1 не прочитанное уведомление.";
                            }
                            else
                            {
                                notifyText = $"У Вас {unreadCount} не прочитанных уведомлений.";
                            }

                            NotifyService.Notify(NotificationSeverity.Info, "Уведомления", $"{notifyText} Нажмите на колокольчик сверху, чтобы посмотреть их!", duration: 5000, click: (NotificationMessage) =>
                            {
                                ToggleDrawer();
                            }, closeOnClick: true);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
            });

            nm.LocationChanged += OnLocationChanged;
        }

        private async Task LoadNotifications()
        {
            var userId = AuthProvider.CurrentUser.Id;
            notifications = await http.GetFromJsonAsync<List<Notification>>($"api/notifications/user/{userId}");
            unreadCount = notifications.Count();
            InvokeAsync(StateHasChanged);
        }

        private async Task ToggleDrawer()
        {
            drawerVisible = !drawerVisible;
            if (drawerVisible)
            {
                await LoadNotifications();
            }
        }

        private async Task CloseAll()
        {
            foreach (var n in notifications)
            {
                await http.PutAsync($"api/notifications/{n.Id}/read", null);
            }

            await LoadNotifications();
        }

        private async Task OnNotificationClick(Notification n)
        {
            if (!string.IsNullOrEmpty(n.Link))
            {
                ToggleDrawer();
                nm.NavigateTo(n.Link);
            }

            await http.PutAsync($"api/notifications/{n.Id}/read", null);

            await LoadNotifications();
        }
    }
}
