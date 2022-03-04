using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamDetailResult;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public partial class Detail : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
    [Parameter] public int TeamId { get; set; }

    private TeamDetailResult? _result;
    private bool _usersTeam = false;
    private bool _isSignedOption = false;
    private string _playerTableHeader = "Rostered Players";
    private IEnumerable<PlayerItem> _playersToDisplay = Array.Empty<PlayerItem>();
    private string _title = string.Empty;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        var authState = await authenticationStateTask;
        var user = authState.User;
        var claim = user.FindFirst("TeamId");
        if (int.Parse(claim.Value) == TeamId) { _usersTeam = true; }
        await LoadDataAsync();
    }
    protected async Task LoadDataAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TeamDetailResult>($"api/teams/{TeamId}", _cts.Token);
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
        _isSignedOption = false;
    }

    private void ShowUnrosteredPlayers()
    {
        _playersToDisplay = _result!.UnrosteredPlayers;
        _playerTableHeader = "Unrostered Players";
        _isSignedOption = false;
    }

    private void ShowUnsignedPlayers()
    {
        _playersToDisplay = _result!.UnsignedPlayers;
        _playerTableHeader = "Unsigned Players";
        _isSignedOption = true;
    }
    private void OpenSignPlayerDialog(int playerId)
    {
        DialogOptions maxWidth = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };
        var parameters = new DialogParameters
        {
            { nameof(SignPlayer.PlayerId), playerId },
            { nameof(SignPlayer.OnSignPlayerButtonClick), EventCallback.Factory.Create(this, () => LoadDataAsync())}
        };

        DialogService.Show<SignPlayer>("Sign Player", parameters, maxWidth);
    }
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
