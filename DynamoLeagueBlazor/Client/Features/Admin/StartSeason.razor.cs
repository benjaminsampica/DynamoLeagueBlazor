using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
public partial class StartSeason
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;

    private bool _isDisabled = true;
    private bool _isProcessing;
    private const string _title = "Start Season";

    protected override async Task OnInitializedAsync()
    {
        _isDisabled = await GetSeasonStatusAsync();
    }

    private async Task StartSeasonAsync()
    {
        _isProcessing = true;
        await HttpClient.PostAsync("api/admin/startseason", null);

        SnackBar.Add("A new season has begun!", Severity.Success);

        _isDisabled = await GetSeasonStatusAsync();
        _isProcessing = false;
    }

    private async Task<bool> GetSeasonStatusAsync()
        => await HttpClient.GetFromJsonAsync<bool>("api/admin/seasonstatus");
}
