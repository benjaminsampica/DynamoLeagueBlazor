using DynamoLeagueBlazor.Shared.Features.Players;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Players;

public partial class List
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private GetPlayerListResult _result = new();
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<GetPlayerListResult>("players") ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }
}
