using DynamoLeagueBlazor.Shared.Features.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Players;

[Authorize]
public partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PlayerListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;
    private const string _title = "Players";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<PlayerListResult>("players", _cts.Token) ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private bool FilterFunc(PlayerListResult.PlayerItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.Name} {item.Position} {item.ContractValue} {item.ContractLength} {item.Team}".Contains(_searchValue))
            return true;
        return false;
    }

    private void OpenAddFineDialog(int playerId)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddFine.PlayerId), playerId }
        };

        DialogService.Show<AddFine>("Add A New Fine", parameters);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
