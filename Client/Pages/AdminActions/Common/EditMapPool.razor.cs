using kTVCSS.Models.Db.Models.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common
{
    public partial class EditMapPool
    {
        RadzenDataGrid<Map> itemsGrid;
        IEnumerable<Map> items;

        DataGridEditMode editMode = DataGridEditMode.Single;

        List<Map> itemsToInsert = new List<Map>();
        List<Map> itemsToUpdate = new List<Map>();

        void Reset()
        {
            itemsToInsert.Clear();
            itemsToUpdate.Clear();
        }

        void Reset(Map item)
        {
            itemsToInsert.Remove(item);
            itemsToUpdate.Remove(item);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            Task.Run(async () =>
            {
                items = await http.GetFromJsonAsync<List<Map>>("/api/mappool");

                await InvokeAsync(StateHasChanged);
            });
        }

        async Task EditRow(Map item)
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            itemsToUpdate.Add(item);
            await itemsGrid.EditRow(item);
        }

        async Task OnUpdateRow(Map item)
        {
            Reset(item);

            await http.PutAsJsonAsync($"/api/mappool/{item.MAP}", item);
        }

        async Task SaveRow(Map item)
        {
            await itemsGrid.UpdateRow(item);
        }

        void CancelEdit(Map item)
        {
            Reset(item);

            itemsGrid.CancelEditRow(item);

            // var itemEntry = dbContext.Entry(item);
            // if (itemEntry.State == EntityState.Modified)
            // {
            //     itemEntry.CurrentValues.SetValues(itemEntry.OriginalValues);
            //     itemEntry.State = EntityState.Unchanged;
            // }
        }

        async Task DeleteRow(Map item)
        {
            Reset(item);

            if (items.Contains(item))
            {
                await http.DeleteAsync($"/api/mappool/{item.MAP}");

                items = await http.GetFromJsonAsync<List<Map>>("/api/mappool");
            }
            else
            {
                itemsGrid.CancelEditRow(item);
                items = await http.GetFromJsonAsync<List<Map>>("/api/mappool");
            }
        }

        async Task InsertRow()
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            var item = new Map();
            itemsToInsert.Add(item);
            await itemsGrid.InsertRow(item);
        }

        async Task InsertAfterRow(Map row)
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            var item = new Map();
            itemsToInsert.Add(item);
            await itemsGrid.InsertAfterRow(item, row);
        }

        async Task OnCreateRow(Map item)
        {
            await http.PostAsJsonAsync("/api/mappool", item);

            itemsToInsert.Remove(item);

            items = await http.GetFromJsonAsync<List<Map>>("/api/mappool");
        }
    }
}
