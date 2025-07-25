﻿using kTVCSSBlazor.Db.Models.Matches;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.Matches
{
    public partial class Matches
    {
        List<TotalMatch> matches = [];
        bool ready = false;
        private int windowHeight = 0;
        private bool isMobile = false;

        void RowRender(RowRenderEventArgs<TotalMatch> args)
        {
            var x = args.Data;

            args.Attributes.Add("class", "row-ktv");
            args.Attributes.Add("Style", "display: grid;margin-bottom: 5px;background-image: url(/images/mapsbackgrs/" + x.MapName + ".jpg);height: " + (isMobile ? "175" : "130") + "px;background-size: 100% auto; background-position: center; background-blend-mode: multiply");
        }

        public async ValueTask DisposeAsync()
        {
            WindowSize.OnResized -= (w, h) => InvokeAsync(StateHasChanged); 
            //await WindowSize.DisposeAsync();
            matches = null;
        }

        protected override async Task OnInitializedAsync()
        {
            Task.Run(async () =>
            {
                isMobile = await mds.IsMobileDeviceAsync();
                WindowSize.OnResized += (w, h) =>
                {
                    Console.WriteLine(h);

                    if (isMobile)
                    {
                        windowHeight = h - 118;
                    }
                    else
                    {
                        windowHeight = h - 208;
                    }

                    InvokeAsync(StateHasChanged);
                };

                windowHeight = WindowSize.GetHeight() - (isMobile ? 118 : 208);

                matches = await http.GetFromJsonAsync<List<TotalMatch>>("/api/matches/getmatches");

                ready = true;

                await InvokeAsync(StateHasChanged);
            });
        }
    }
}
