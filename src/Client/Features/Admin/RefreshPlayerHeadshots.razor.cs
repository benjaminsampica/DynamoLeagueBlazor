using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
public sealed partial class RefreshPlayerHeadshots
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar SnackBar { get; set; }

    private bool _isDisabled;
    private const string _title = "Refresh Player Headshots";

    private async Task RefreshPlayerHeadshotsAsync()
    {
        _isDisabled = true;

        await HttpClient.PostAsync(RefreshPlayerHeadshotsRouteFactory.Uri, null);

        SnackBar.Add("Player headshots refreshed!", Severity.Success);

        _isDisabled = false;
    }
}
