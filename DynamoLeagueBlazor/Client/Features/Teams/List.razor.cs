using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Teams;

[Authorize]
public partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private TeamListResult? _result;
    private const string _title = "Teams";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TeamListResult>("api/teams", _cts.Token);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
