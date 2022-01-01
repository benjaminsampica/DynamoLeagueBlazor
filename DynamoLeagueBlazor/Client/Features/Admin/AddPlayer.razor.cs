using DynamoLeagueBlazor.Shared.Features.Players;
using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Admin;

public partial class AddPlayer : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    private TeamNameListResult _teamList = new TeamNameListResult();
    private AddPlayerRequest _form = new AddPlayerRequest();
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _teamList = await HttpClient.GetFromJsonAsync<TeamNameListResult>("admin/addplayer", _cts.Token);
            
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
            var response = await HttpClient.PostAsJsonAsync("admin/addplayer", _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Player Succesfully added", Severity.Success);
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
