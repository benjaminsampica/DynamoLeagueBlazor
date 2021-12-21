using DynamoLeagueBlazor.Shared.Infastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Admin;

[Authorize(Roles = RoleName.Admin)]
public partial class StartSeason
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;

    private bool _isDisabled = true;
    private bool _isProcessing;

    protected override async Task OnInitializedAsync()
    {
        _isDisabled = await GetSeasonStatusAsync();
    }

    private async Task StartSeasonAsync()
    {
        _isProcessing = true;
        await HttpClient.PostAsync("admin/startseason", null);

        SnackBar.Add("A new season has begun!", Severity.Success);

        _isDisabled = await GetSeasonStatusAsync();
        _isProcessing = false;
    }

    private async Task<bool> GetSeasonStatusAsync()
        => await HttpClient.GetFromJsonAsync<bool>("admin/seasonstatus");
}
