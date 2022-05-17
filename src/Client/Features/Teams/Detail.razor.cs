using DynamoLeagueBlazor.Shared.Features.Teams;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamDetailResult;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class Detail : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Parameter] public int TeamId { get; set; }

    private TeamDetailResult? _result;
    private bool _isUsersTeam = false;
    private bool _isSignedPlayersShowing = false;
    private string _playerTableHeader = "Rostered Players";
    private IEnumerable<PlayerItem> _playersToDisplay = Array.Empty<PlayerItem>();
    private string _title = string.Empty;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateTask;
        var user = authenticationState.User!;

        var claim = user.FindFirst(nameof(IUser.TeamId));
        if (int.Parse(claim!.Value) == TeamId) _isUsersTeam = true;

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TeamDetailResult>(TeamDetailRouteFactory.Create(TeamId), _cts.Token);
            _title = $"Team Detail - {_result!.Name}";
            ShowRosteredPlayers();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private void ShowRosteredPlayers()
    {
        _playersToDisplay = _result!.RosteredPlayers;
        _playerTableHeader = "Rostered Players";
        _isSignedPlayersShowing = false;
    }

    private void ShowUnrosteredPlayers()
    {
        _playersToDisplay = _result!.UnrosteredPlayers;
        _playerTableHeader = "Unrostered Players";
        _isSignedPlayersShowing = false;
    }

    private void ShowUnsignedPlayers()
    {
        _playersToDisplay = _result!.UnsignedPlayers;
        _playerTableHeader = "Unsigned Players";
        _isSignedPlayersShowing = true;
    }

    private async void OpenSignPlayerDialog(int playerId)
    {
        var maxWidth = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };
        var parameters = new DialogParameters
        {
            { nameof(SignPlayer.PlayerId), playerId }
        };

        var dialog = DialogService.Show<SignPlayer>("Sign Player", parameters, maxWidth);
        var result = await dialog.Result;


        if (!result.Cancelled)
        {
            await LoadDataAsync();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
