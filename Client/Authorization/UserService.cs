using Blazored.LocalStorage;
using kTVCSS.Models.Models;
using kTVCSSBlazor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace kTVCSSBlazor.Client.Authorization
{
    public class UserService
    {
        private const string _storageKey = "kTVCSSIdentityv11032025";
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorage;
        private HttpClient _http;

        public UserService(IConfiguration configuration, ILocalStorageService localStorage, HttpClient http)
        {
            _configuration = configuration;
            _localStorage = localStorage;
            _http = http;
        }

        public async Task<RegisterResult> Register(string username, string password)
        {
            return await _http.GetFromJsonAsync<RegisterResult>($"/api/register?username={username}&password={password}");
        }

        private async Task<User?> Get(string username, string password)
        {
            try
            {
                var user = await _http.GetFromJsonAsync<User>($"/api/login?username={username}&password={password}");
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

        public async Task<User?> LookupUserInDatabaseAsync(string username, string password)
        {
            var task = Get(username, password);            

            await Task.WhenAll(task);

            return task.Result;
        }

        public async Task PersistUserToBrowserAsync(User user)
        {
            string userJson = JsonSerializer.Serialize(user);
            _localStorage.SetItemAsStringAsync(_storageKey, userJson);
        }

        public async Task<User?> FetchUserFromBrowserAsync()
        {
            try
            {
                var storedUserResult = await _localStorage.GetItemAsync<string>(_storageKey);

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

        public async Task ClearBrowserUserDataAsync() => _localStorage.RemoveItemAsync(_storageKey);
    }
}
