using Blazored.LocalStorage;
using kTVCSS.Models.Models;
using kTVCSSBlazor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Authorization
{
    public class UserService
    {
        private const string _storageKey = "kTVCSSComIdent092025";
        private readonly IServiceProvider _serviceProvider;

        public UserService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<RegisterResult> Register(string username, string password)
        {
            return await GetHTTPClient().GetFromJsonAsync<RegisterResult>($"/api/register?username={username}&password={password}");
        }

        private ILocalStorageService GetLocalStorage()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILocalStorageService>();
        }

        private HttpClient GetHTTPClient()
        {

#if DEBUG
            return new HttpClient { BaseAddress = new Uri("http://localhost:3000"), Timeout = TimeSpan.FromMinutes(1) };
#endif

#if RELEASE
            return new HttpClient { BaseAddress = new Uri("https://api.ktvcss.com"), Timeout = TimeSpan.FromMinutes(1) };
#endif

        }

        private async Task<User?> Get(LoginArgs args)
        {
            try
            {
                var response = await GetHTTPClient().PostAsJsonAsync($"/api/login", args);

                response.EnsureSuccessStatusCode();

                var user = await response.Content.ReadFromJsonAsync<User>();

                if (user is not null)
                {
                    return user;
                }
            }
            catch (Exception)
            {
                // null
            }
            return null;
        }

        public async Task<User?> LookupUserInDatabaseAsync(LoginArgs args)
        {
            var task = Get(args);            

            await Task.WhenAll(task);

            return task.Result;
        }

        public async Task PersistUserToBrowserAsync(User user)
        {
            string userJson = JsonSerializer.Serialize(user);
            GetLocalStorage().SetItemAsStringAsync(_storageKey, userJson);
        }

        public async Task<User?> FetchUserFromBrowserAsync()
        {
            try
            {
                var storedUserResult = await GetLocalStorage().GetItemAsync<string>(_storageKey);

                if (!string.IsNullOrEmpty(storedUserResult))
                {
                    var user = JsonSerializer.Deserialize<User>(storedUserResult);

                    return user;
                }
            }
            catch (InvalidOperationException)
            {

            }

            return null;
        }

        public async Task ClearBrowserUserDataAsync() => GetLocalStorage().RemoveItemAsync(_storageKey);
    }
}
