using kTVCSS.Models.Models;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Common
{
    public partial class Downloads
    {
        private bool ready = false;
        List<FastDLItem> files = [];

        public async Task DownloadFileAsync(int id, string fileName)
        {
            var response = await http.GetAsync(
                $"api/fastdl/GetFile?id={id}",
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            using var dotNetStreamRef = new DotNetStreamReference(stream: stream);

            await js.InvokeVoidAsync(
                "saveFileFromStream",
                fileName,
                dotNetStreamRef
            );
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                files = await http.GetFromJsonAsync<List<FastDLItem>>("/api/fastdl/getfiles");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
