using DynamoLeagueBlazor.Shared.Features.Fines;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Dashboard;

public partial class Fines
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private FineListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<FineListResult>("fines") ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private bool FilterFunc(FineListResult.FineItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.PlayerName} {item.FineReason} {item.FineAmount} {item.FineStatus}".Contains(_searchValue))
            return true;
        return false;
    }
}
