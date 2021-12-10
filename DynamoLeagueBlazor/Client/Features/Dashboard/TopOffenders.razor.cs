using DynamoLeagueBlazor.Shared.Features.Fines;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Dashboard;

public partial class TopOffenders
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private GetFineListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<GetFineListResult>("fines") ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }
}
