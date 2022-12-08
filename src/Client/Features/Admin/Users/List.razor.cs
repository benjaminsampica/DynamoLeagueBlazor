using DynamoLeagueBlazor.Shared.Features.Admin.Users;
using DynamoLeagueBlazor.Shared.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Admin.Users;

[Authorize(Policy = PolicyRequirements.Admin)]
public sealed partial class List : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }

    private UserListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;
    private const string _title = "Users";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _loading = true;
        _result = await HttpClient.GetFromJsonAsync<UserListResult>("api/admin/users", _cts.Token) ?? new();
        _loading = false;
    }

    private bool FilterFunc(UserListResult.UserItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.Email} {item.Team}".Contains(_searchValue))
            return true;
        return false;
    }

    private void OpenDeleteUserDialog(string userId)
    {
        var parameters = new DialogParameters
        {
            { nameof(Delete.UserId), userId }
        };

        DialogService.Show<Delete>("Delete User", parameters);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
