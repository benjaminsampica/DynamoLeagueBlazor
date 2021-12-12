using DynamoLeagueBlazor.Shared.Features.Dashboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Dashboard.TopOffenders;

public partial class TopOffenders
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private TopOffendersResult? _result;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TopOffendersResult>("dashboard/topoffenders");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }
}
