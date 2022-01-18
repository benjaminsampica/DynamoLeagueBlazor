using DynamoLeagueBlazor.Shared.Features.Dashboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Dashboard.TopOffenders;

public partial class TopOffenders : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private TopOffendersResult? _result;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TopOffendersResult>("api/dashboard/topoffenders", _cts.Token);
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
