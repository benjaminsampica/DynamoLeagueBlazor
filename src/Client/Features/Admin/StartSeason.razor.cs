using DynamoLeagueBlazor.Client.Shared.Components;
using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
public sealed partial class StartSeason : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar SnackBar { get; set; }
    [Inject] public required IConfirmDialogService ConfirmDialogService { get; set; }


    private bool _isDisabled = true;
    private const string _title = "Start Season";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _isDisabled = await GetSeasonStatusAsync();
    }

    private async Task StartSeasonAsync()
    {
        if (await ConfirmDialogService.IsCancelledAsync()) return;

        await HttpClient.PostAsync(StartSeasonRouteFactory.Uri, null);

        SnackBar.Add("A new season has begun!", Severity.Success);

        _isDisabled = await GetSeasonStatusAsync();
    }

    private async Task<bool> GetSeasonStatusAsync()
        => await HttpClient.GetFromJsonAsync<bool>(StartSeasonRouteFactory.Uri, _cts.Token);

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
