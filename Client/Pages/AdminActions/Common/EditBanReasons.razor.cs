using kTVCSS.Models.Db.Models.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common
{
    public partial class EditBanReasons
    {
        RadzenDataGrid<BanReason> itemsGrid;
        IEnumerable<BanReason> items;

        DataGridEditMode editMode = DataGridEditMode.Single;

        List<BanReason> itemsToInsert = new List<BanReason>();
        List<BanReason> itemsToUpdate = new List<BanReason>();

        void Reset()
        {
            itemsToInsert.Clear();
            itemsToUpdate.Clear();
        }

        void Reset(BanReason item)
        {
            itemsToInsert.Remove(item);
            itemsToUpdate.Remove(item);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            Task.Run(async () =>
            {
                items = await http.GetFromJsonAsync<List<BanReason>>("/api/banreasons");

                await InvokeAsync(StateHasChanged);
            });
        }

        async Task EditRow(BanReason item)
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            itemsToUpdate.Add(item);
            await itemsGrid.EditRow(item);
        }

        async Task OnUpdateRow(BanReason item)
        {
            Reset(item);

            await http.PutAsJsonAsync($"/api/banreasons/{item.Id}", item);
        }

        async Task SaveRow(BanReason item)
        {
            await itemsGrid.UpdateRow(item);
        }

        void CancelEdit(BanReason item)
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

        async Task DeleteRow(BanReason item)
        {
            Reset(item);

            if (items.Contains(item))
            {
                await http.DeleteAsync($"/api/banreasons/{item.Id}");

                items = await http.GetFromJsonAsync<List<BanReason>>("/api/banreasons");
            }
            else
            {
                itemsGrid.CancelEditRow(item);
                items = await http.GetFromJsonAsync<List<BanReason>>("/api/banreasons");
            }
        }

        async Task InsertRow()
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            var item = new BanReason();
            itemsToInsert.Add(item);
            await itemsGrid.InsertRow(item);
        }

        async Task InsertAfterRow(BanReason row)
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            var item = new BanReason();
            itemsToInsert.Add(item);
            await itemsGrid.InsertAfterRow(item, row);
        }

        async Task OnCreateRow(BanReason item)
        {
            await http.PostAsJsonAsync("/api/banreasons", item);

            itemsToInsert.Remove(item);

            items = await http.GetFromJsonAsync<List<BanReason>>("/api/banreasons");
        }
    }
}
