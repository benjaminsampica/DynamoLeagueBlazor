using DynamoLeagueBlazor.Shared.Features.Fines;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Fines;

public partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private FineListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;
    private const string _title = "Fines";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<FineListResult>("fines", _cts.Token) ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private bool FilterFunc(FineListResult.FineItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.PlayerName} {item.FineReason} {item.FineAmount} {item.FineStatus}".Contains(_searchValue))
            return true;
        return false;
    }

    private void OpenManageFineDialog(int fineId)
    {
        var parameters = new DialogParameters
        {
            { nameof(Manage.FineId), fineId },
            { nameof(Manage.OnManageButtonClick), EventCallback.Factory.Create(this, () => LoadDataAsync())}
        };

        DialogService.Show<Manage>("Manage Fine", parameters);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
