using System;
using System.Net.Http.Json;
using kTVCSS.Models.Db.Models.Common;
using Radzen;
using Radzen.Blazor;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common;

public partial class EditCitations
{
    RadzenDataGrid<Citation> itemsGrid;
    IEnumerable<Citation> items;

    DataGridEditMode editMode = DataGridEditMode.Single;

    List<Citation> itemsToInsert = new List<Citation>();
    List<Citation> itemsToUpdate = new List<Citation>();

    void Reset()
    {
        itemsToInsert.Clear();
        itemsToUpdate.Clear();
    }

    void Reset(Citation item)
    {
        itemsToInsert.Remove(item);
        itemsToUpdate.Remove(item);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Task.Run(async () =>
        {
            items = await http.GetFromJsonAsync<List<Citation>>("/api/Citations");

            await InvokeAsync(StateHasChanged);
        });
    }

    async Task EditRow(Citation item)
    {
        if (!itemsGrid.IsValid) return;

        if (editMode == DataGridEditMode.Single)
        {
            Reset();
        }

        itemsToUpdate.Add(item);
        await itemsGrid.EditRow(item);
    }

    async Task OnUpdateRow(Citation item)
    {
        Reset(item);

        //await http.PutAsJsonAsync($"/api/Citations/{item.Citation}", item);
    }

    async Task SaveRow(Citation item)
    {
        await itemsGrid.UpdateRow(item);
    }

    void CancelEdit(Citation item)
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

    async Task DeleteRow(Citation item)
    {
        Reset(item);

        if (items.Contains(item))
        {
            await http.DeleteAsync($"/api/Citations?id={item.Id}");

            items = await http.GetFromJsonAsync<List<Citation>>("/api/Citations");
        }
        else
        {
            itemsGrid.CancelEditRow(item);
            items = await http.GetFromJsonAsync<List<Citation>>("/api/Citations");
        }
    }

    async Task InsertRow()
    {
        if (!itemsGrid.IsValid) return;

        if (editMode == DataGridEditMode.Single)
        {
            Reset();
        }

        var item = new Citation();
        itemsToInsert.Add(item);
        await itemsGrid.InsertRow(item);
    }

    async Task InsertAfterRow(Citation row)
    {
        if (!itemsGrid.IsValid) return;

        if (editMode == DataGridEditMode.Single)
        {
            Reset();
        }

        var item = new Citation();
        itemsToInsert.Add(item);
        await itemsGrid.InsertAfterRow(item, row);
    }

    async Task OnCreateRow(Citation item)
    {
        await http.PostAsJsonAsync("/api/Citations", item);

        itemsToInsert.Remove(item);

        items = await http.GetFromJsonAsync<List<Citation>>("/api/Citations");
    }
}
