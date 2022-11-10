using DynamoLeagueBlazor.Shared.Features.Fines;

namespace DynamoLeagueBlazor.Client.Features.Fines;

public sealed partial class List : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }

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
        _loading = true;
        _result = await HttpClient.GetFromJsonAsync<FineListResult>(FineListRouteFactory.Uri, _cts.Token) ?? new();
        _loading = false;
    }

    private bool FilterFunc(FineListResult.FineItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.PlayerName} {item.Reason} {item.Amount} {item.Status}".Contains(_searchValue))
            return true;
        return false;
    }

    private async void OpenManageFineDialog(int fineId)
    {
        var parameters = new DialogParameters
        {
            { nameof(Manage.FineId), fineId },
        };

        var dialog = DialogService.Show<Manage>("Manage Fine", parameters);

        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
