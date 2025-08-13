using System;
using System.Net.Http.Json;
using kTVCSS.Models.Db.Models.Roles;
using Radzen;
using Radzen.Blazor;

namespace kTVCSSBlazor.Client.Pages.AdminActions.Common;

public partial class EditAdmins
{
    RadzenDataGrid<Role> itemsGrid;
    IEnumerable<Role> items;

    DataGridEditMode editMode = DataGridEditMode.Single;

    List<Role> itemsToInsert = new List<Role>();
    List<Role> itemsToUpdate = new List<Role>();

    void Reset()
    {
        itemsToInsert.Clear();
        itemsToUpdate.Clear();
    }

    void Reset(Role item)
    {
        itemsToInsert.Remove(item);
        itemsToUpdate.Remove(item);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Task.Run(async () =>
        {
            items = await http.GetFromJsonAsync<List<Role>>("/api/admins/GetAdmins");

            await InvokeAsync(StateHasChanged);
        });
    }

    async Task EditRow(Role item)
    {
        if (!itemsGrid.IsValid) return;

        if (editMode == DataGridEditMode.Single)
        {
            Reset();
        }

        itemsToUpdate.Add(item);
        await itemsGrid.EditRow(item);
    }

    async Task OnUpdateRow(Role item)
    {
        Reset(item);

        await http.PutAsJsonAsync($"/api/admins/EditAdmin?id={item.Id}", item);
    }

    async Task SaveRow(Role item)
    {
        await itemsGrid.UpdateRow(item);
    }

    void CancelEdit(Role item)
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

    async Task DeleteRow(Role item)
    {
        Reset(item);

        if (items.Contains(item))
        {
            await http.DeleteAsync($"/api/admins/removeadmin?id={item.Id}");

            items = await http.GetFromJsonAsync<List<Role>>("/api/admins/GetAdmins");
        }
        else
        {
            itemsGrid.CancelEditRow(item);
            items = await http.GetFromJsonAsync<List<Role>>("/api/admins/GetAdmins");
        }
    }

    async Task InsertRow()
    {
        if (!itemsGrid.IsValid) return;

        if (editMode == DataGridEditMode.Single)
        {
            Reset();
        }

        var item = new Role();
        itemsToInsert.Add(item);
        await itemsGrid.InsertRow(item);
    }

    async Task InsertAfterRow(Role row)
    {
        if (!itemsGrid.IsValid) return;

        if (editMode == DataGridEditMode.Single)
        {
            Reset();
        }

        var item = new Role();
        itemsToInsert.Add(item);
        await itemsGrid.InsertAfterRow(item, row);
    }

    async Task OnCreateRow(Role item)
    {
        await http.PostAsJsonAsync("/api/admins/AddAdmin", item);

        itemsToInsert.Remove(item);

        items = await http.GetFromJsonAsync<List<Role>>("/api/admins/GetAdmins");
    }
}
