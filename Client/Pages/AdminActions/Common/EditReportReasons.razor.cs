using kTVCSS.Models.Db.Models.Common;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common
{
    public partial class EditReportReasons
    {
        RadzenDataGrid<ReportReason> itemsGrid;
        IEnumerable<ReportReason> items;

        DataGridEditMode editMode = DataGridEditMode.Single;

        List<ReportReason> itemsToInsert = new List<ReportReason>();
        List<ReportReason> itemsToUpdate = new List<ReportReason>();

        void Reset()
        {
            itemsToInsert.Clear();
            itemsToUpdate.Clear();
        }

        void Reset(ReportReason item)
        {
            itemsToInsert.Remove(item);
            itemsToUpdate.Remove(item);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            Task.Run(async () =>
            {
                items = await http.GetFromJsonAsync<List<ReportReason>>("/api/ReportReasons");

                await InvokeAsync(StateHasChanged);
            });
        }

        async Task EditRow(ReportReason item)
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            itemsToUpdate.Add(item);
            await itemsGrid.EditRow(item);
        }

        async Task OnUpdateRow(ReportReason item)
        {
            Reset(item);

            await http.PutAsJsonAsync($"/api/ReportReasons/{item.Id}", item);
        }

        async Task SaveRow(ReportReason item)
        {
            await itemsGrid.UpdateRow(item);
        }

        void CancelEdit(ReportReason item)
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

        async Task DeleteRow(ReportReason item)
        {
            Reset(item);

            if (items.Contains(item))
            {
                await http.DeleteAsync($"/api/ReportReasons/{item.Id}");

                items = await http.GetFromJsonAsync<List<ReportReason>>("/api/ReportReasons");
            }
            else
            {
                itemsGrid.CancelEditRow(item);
                items = await http.GetFromJsonAsync<List<ReportReason>>("/api/ReportReasons");
            }
        }

        async Task InsertRow()
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            var item = new ReportReason();
            itemsToInsert.Add(item);
            await itemsGrid.InsertRow(item);
        }

        async Task InsertAfterRow(ReportReason row)
        {
            if (!itemsGrid.IsValid) return;

            if (editMode == DataGridEditMode.Single)
            {
                Reset();
            }

            var item = new ReportReason();
            itemsToInsert.Add(item);
            await itemsGrid.InsertAfterRow(item, row);
        }

        async Task OnCreateRow(ReportReason item)
        {
            await http.PostAsJsonAsync("/api/ReportReasons", item);

            itemsToInsert.Remove(item);

            items = await http.GetFromJsonAsync<List<ReportReason>>("/api/ReportReasons");
        }
    }
}
