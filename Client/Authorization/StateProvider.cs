using kTVCSS.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using System.Security.Claims;

namespace kTVCSSBlazor.Client.Authorization
{
    public class StateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly UserService _userService;
        private readonly IJSRuntime _js;
        public User CurrentUser { get; private set; }

        public StateProvider(UserService userService, IJSRuntime js)
        {
            _js = js;
            _userService = userService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();
            var user = await _userService.FetchUserFromBrowserAsync();

            if (user is not null)
            {
                var userInDatabase = await _userService.LookupUserInDatabaseAsync(new LoginArgs() { Username = user.Username, Password = user.Password });

                if (userInDatabase is not null)
                {
                    principal = userInDatabase.ToClaimsPrincipal();
                    CurrentUser = userInDatabase;
                }
            }

            return new(principal);
        }

        private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
        {
            var authenticationState = await task;

            if (authenticationState is not null)
            {
                CurrentUser = User.FromClaimsPrincipal(authenticationState.User);
            }
        }

        public async Task<User?> GetUserFromDataBase()
        {
            var localStorageData = await _userService.FetchUserFromBrowserAsync();
            if (localStorageData != null)
            {
                return await _userService.LookupUserInDatabaseAsync(new LoginArgs() { Username = localStorageData.Username, Password = localStorageData.Password });
            }
            else return null;
        }

        public async Task<User?> GetUserFromLocalStorage()
        {
            return await _userService.FetchUserFromBrowserAsync();
        }

        public async Task<string> LoginAsync(LoginArgs args)
        {
            string error = string.Empty;
            var principal = new ClaimsPrincipal();
            
            try
            {
                var user = await _userService.LookupUserInDatabaseAsync(args);

                if (user is not null)
                {
                    await _userService.PersistUserToBrowserAsync(user);
                    principal = user.ToClaimsPrincipal();
                }
                else
                {
                    error = "Неверный логин или пароль!";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

            return error;
        }

        public async Task<RegisterResult> SignupAsync(string username, string password)
        {
            return await _userService.Register(username, password);
        }

        public async Task LogoutAsync()
        {
            await _userService.ClearBrowserUserDataAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
        }

        public void Dispose() => AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;
    }
}
