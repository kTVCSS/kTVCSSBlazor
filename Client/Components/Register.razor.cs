using kTVCSS.Models.Models;
using Radzen;
using System.Text.RegularExpressions;

namespace kTVCSSBlazor.Client.Components
{
    public partial class Register
    {
        string userName = "";
        string password = "";
        bool rememberMe = true;

        public bool IsValidLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
                return false;

            if (login.Length > 30)
                return false;

            // Регулярное выражение: от 1 до 30 символов, каждый из которых – буква, цифра, '_' или '-'
            const string pattern = @"^[A-Za-z0-9_-]+$";
            return Regex.IsMatch(login, pattern);
        }

        private async Task OnRegister(LoginArgs args, string name)
        {
            if (!IsValidLogin(args.Username))
            {
                notify.Notify(new NotificationMessage()
                {
                    CloseOnClick = true,
                    Detail = "Логин содержит недопустимые знаки!",
                    Summary = "Ошибка регистрации",
                    Severity = NotificationSeverity.Error,
                    Duration = 5000,
                    Payload = DateTime.Now
                });

                return;
            }

            RegisterResult result = await AuthProvider.SignupAsync(args.Username, args.Password);

            if (result != RegisterResult.Ok)
            {
                if (result == RegisterResult.Exist)
                {
                    notify.Notify(new NotificationMessage()
                    {
                        CloseOnClick = true,
                        Detail = "Такой логин уже используется",
                        Summary = "Ошибка регистрации",
                        Severity = NotificationSeverity.Error,
                        Duration = 5000,
                        Payload = DateTime.Now
                    });
                }

                return;
            }

            await AuthProvider.LoginAsync(args);

            notify.Notify(NotificationSeverity.Success, "Успех", $"Благодарим за регистрацию, {args.Username}. Теперь можете войти в свою учетную запись!", TimeSpan.FromSeconds(5));

            await Task.Delay(2500);

            NavigationManager.NavigateTo("/login", true);

            StateHasChanged();
        }
    }
}
