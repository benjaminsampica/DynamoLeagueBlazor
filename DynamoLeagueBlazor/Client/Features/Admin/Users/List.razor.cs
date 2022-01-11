using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Admin.Users;

[Authorize(Policy = PolicyRequirements.Admin)]
public partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

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
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<UserListResult>("admin/users", _cts.Token) ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private bool FilterFunc(UserListResult.UserItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.Email} {item.Team}".Contains(_searchValue))
            return true;
        return false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
