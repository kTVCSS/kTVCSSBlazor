using kTVCSS.Models.Db.Models.Common;
using kTVCSSBlazor.Db.Models.Home;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Components.Home
{
    public partial class UserStreams
    {
        List<UserStream> _streams = [];

        protected override async Task OnInitializedAsync()
        {
            _streams = await http.GetFromJsonAsync<List<UserStream>>("/api/userstreams");

            foreach (var stream in _streams)
            {
                stream.Channel = $"https://player.twitch.tv/?channel={stream.Name}&parent=ktvcss.com";
            }
        }

        private string streamName = string.Empty;
        private string streamLink = string.Empty;
        private string profileId;

        private async Task RemoveStream(int id)
        {
            await http.DeleteAsync("/api/userstreams/" + id);

            await OnInitializedAsync();

            await InvokeAsync(StateHasChanged);
        }

        private async Task AddStream()
        {
            var stream = new UserStream()
            {
                Channel = streamLink,
                Name = streamName,
                Type = StreamType.Twitch,
                Url = streamLink,
                PlayerId = int.Parse(profileId)
            };

            await http.PostAsJsonAsync("/api/userstreams", stream);

            await OnInitializedAsync();

            await InvokeAsync(StateHasChanged);
        }
    }
}
