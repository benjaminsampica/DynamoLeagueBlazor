using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
public sealed partial class AddPlayer : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;

    private TeamNameListResult _teamList = new();
    private AddPlayerRequest _form = new();
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _teamList = await HttpClient.GetFromJsonAsync<TeamNameListResult>(AddPlayerRouteFactory.Uri, _cts.Token) ?? new TeamNameListResult();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync(AddPlayerRouteFactory.Uri, _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Player successfully added.", Severity.Success);
                _form = new();
            }
            else
            {
                SnackBar.Add("Something went wrong...", Severity.Error);
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        _processingForm = false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public static class AddPlayerRouteFactory
{
    public const string Uri = "api/admin/addplayer";
}
