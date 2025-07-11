using kTVCSSBlazor.Db.Models.Matches;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Players
{
    public partial class MyMatches
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Id { get; set; }

        List<TotalMatch> matches = [];
        bool ready = false;
        double dataGridHeight = 77;

        void RowRender(RowRenderEventArgs<TotalMatch> args)
        {
            var x = args.Data;

            args.Attributes.Add("class", "row-ktv");
            args.Attributes.Add("Style", "display: grid;margin-bottom: 5px;background-image: url(/images/mapsbackgrs/" + x.MapName + ".jpg);height: 130px;background-size: 100% auto; background-position: center; background-blend-mode: multiply");
        }

        public void Dispose()
        {
            matches = null;
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                matches = await http.GetFromJsonAsync<List<TotalMatch>>("/api/players/getmymatches?id=" + Id);

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
