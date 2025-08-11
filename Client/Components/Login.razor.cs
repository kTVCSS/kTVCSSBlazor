using System.Text;
using Radzen;

namespace kTVCSSBlazor.Client.Components
{
    public partial class Login
    {
        string userName = "";
        string password = "";
        bool rememberMe = true;

        private async Task OnLogin(LoginArgs args, string name)
        {
            var tArgs = args;
            tArgs.Password = MD5.Calculate(Encoding.ASCII.GetBytes(args.Password));

            string result = await AuthProvider.LoginAsync(tArgs);

            if (!string.IsNullOrEmpty(result))
            {
                notify.Notify(new NotificationMessage()
                {
                    CloseOnClick = true,
                    Detail = result,
                    Summary = "Ошибка авторизации",
                    Severity = NotificationSeverity.Error,
                    Duration = 5000,
                    Payload = DateTime.Now
                });
                return;
            }

            await Task.Delay(1500);

            NavigationManager.NavigateTo("/", true);

            StateHasChanged();
        }

        void OnRegister(string name)
        {
            NavigationManager.NavigateTo("/register");
        }

        void OnResetPassword(string value, string name)
        {
            notify.Notify(NotificationSeverity.Info, "Долбаеб", $"Нехуй забывать пароль", TimeSpan.FromSeconds(5));
        }
    }
}
