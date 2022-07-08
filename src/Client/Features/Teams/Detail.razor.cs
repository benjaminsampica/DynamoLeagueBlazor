using DynamoLeagueBlazor.Client.Shared.Components;
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
    [Inject] private IConfirmDialogService ConfirmDialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] public Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Parameter, EditorRequired] public int TeamId { get; set; }

    private TeamDetailResult? _result;
    private bool _isUsersTeam = false;
    private string _playerTableHeader = "Rostered Players";
    private IEnumerable<PlayerItem> _playersToDisplay = Array.Empty<PlayerItem>();
    private string _title = string.Empty;
    private readonly CancellationTokenSource _cts = new();
    private Func<int, Task>? _onPlayerTableActionClick;
    private string? _tableActionIcon;

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateTask;
        var user = authenticationState.User!;

        var claim = user.FindFirst(nameof(IUser.TeamId));
        _isUsersTeam = int.Parse(claim!.Value) == TeamId;

        await LoadDataAsync();
        ShowRosteredPlayers();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TeamDetailResult>(TeamDetailRouteFactory.Create(TeamId), _cts.Token);
            _title = $"Team Detail - {_result!.Name}";
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
        _onPlayerTableActionClick = UnrosterPlayerAsync;
        _tableActionIcon = Icons.Outlined.PersonRemove;
    }

    private void ShowUnrosteredPlayers()
    {
        _playersToDisplay = _result!.UnrosteredPlayers;
        _playerTableHeader = "Unrostered Players";
        _onPlayerTableActionClick = null;
    }

    private void ShowUnsignedPlayers()
    {
        _playersToDisplay = _result!.UnsignedPlayers;
        _playerTableHeader = "Unsigned Players";
        _onPlayerTableActionClick = OpenSignPlayerDialogAsync;
        _tableActionIcon = Icons.Filled.AssignmentLate;
    }

    private async Task UnrosterPlayerAsync(int playerId)
    {
        if (await ConfirmDialogService.IsCancelledAsync()) return;

        try
        {
            var response = await HttpClient.PostAsJsonAsync(DropPlayerRouteFactory.Uri, new DropPlayerRequest { PlayerId = playerId }, _cts.Token);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Successfully unrostered player.", Severity.Success);

                await LoadDataAsync();
                ShowRosteredPlayers();
            }
            else
            {
                Snackbar.Add("Something went wrong...", Severity.Error);
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private async Task OpenSignPlayerDialogAsync(int playerId)
    {
        var dialogOptions = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };
        var parameters = new DialogParameters
        {
            { nameof(SignPlayer.PlayerId), playerId }
        };

        var dialog = DialogService.Show<SignPlayer>("Sign Player", parameters, dialogOptions);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            await LoadDataAsync();
            ShowUnsignedPlayers();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        _onPlayerTableActionClick = null;
    }
}
