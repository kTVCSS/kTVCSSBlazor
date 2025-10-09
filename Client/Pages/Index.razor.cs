using kTVCSS.Models.Db.Models.Common;
using kTVCSS.Models.Models;
using kTVCSSBlazor.Db.Models.Arcticles;
using kTVCSSBlazor.Db.Models.Home;
using kTVCSSBlazor.Db.Models.UserFeed;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages
{
    public partial class Index
    {
        private bool ready = false;
        private int _selectedIndex = 0;

        private int selectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;

                NavigationManager.NavigateTo($"/#{GetSectionName(value)}");
            }
        }

        private string GetSectionName(int index)
        {
            switch (index)
            {
                case 0: return "wall";
                case 1: return "news";
                case 2: return "streams";
                case 3: return "videos";
                case 4: return "configs";
                case 5: return "suggestions";
                case 6: return "halloffame";
                default: return "";
            }
        }

        private int GetSectionIndex(string name)
        {
            switch (name)
            {
                case "wall": return 0;
                case "news": return 1;
                case "streams": return 2;
                case "videos": return 3;
                case "configs": return 4;
                case "suggestions": return 5;
                case "halloffame": return 6;
                default: return 0;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
                var fragment = uri.Fragment;

                if (!string.IsNullOrEmpty(fragment))
                {
                    var sectionId = fragment.TrimStart('#');

                    selectedIndex = GetSectionIndex(sectionId);

                    await InvokeAsync(StateHasChanged);
                }

                ready = true;

                await InvokeAsync(StateHasChanged);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
