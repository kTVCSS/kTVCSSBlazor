using kTVCSSBlazor.Db.Models.Matches;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Matches
{
    public partial class Matches
    {
        List<TotalMatch> matches = [];
        double dataGridHeight = 81;
        bool ready = false;

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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Task.Run(async () =>
                {
                    matches = await http.GetFromJsonAsync<List<TotalMatch>>("/api/matches/getmatches");

                    ready = true;

                    await InvokeAsync(StateHasChanged);
                });
            }
        }
    }
}
