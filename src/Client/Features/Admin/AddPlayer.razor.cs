using DynamoLeagueBlazor.Shared.Features.Admin;
using DynamoLeagueBlazor.Shared.Features.Admin.Shared;
using DynamoLeagueBlazor.Shared.Infastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DynamoLeagueBlazor.Client.Features.Admin;

[Authorize(Policy = PolicyRequirements.Admin)]
public sealed partial class AddPlayer : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar SnackBar { get; set; }

    private TeamNameListResult _teamList = new();
    private AddPlayerRequest _addPlayerForm = new();
    private readonly PlayerPreviewRequest _playerPreviewForm = new();
    private string _previewHeadshotUrl = string.Empty;
    private bool _isPreviewButtonDisabled;
    private bool _isProcessingForm;
    private const string _title = "Add Player";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _teamList = await HttpClient.GetFromJsonAsync<TeamNameListResult>(AddPlayerRouteFactory.GetTeamListUri, _cts.Token) ?? new TeamNameListResult();
    }

    private async Task OnValidSubmitAsync()
    {
        _isProcessingForm = true;

        var response = await HttpClient.PostAsJsonAsync(AddPlayerRouteFactory.Uri, _addPlayerForm);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Player successfully added.", Severity.Success);
            _addPlayerForm = new();
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }

        _isProcessingForm = false;
    }

    private async Task PreviewHeadshotAsync()
    {
        _isPreviewButtonDisabled = true;

        var response = await HttpClient.GetFromJsonAsync<PlayerPreviewResponse>(AddPlayerRouteFactory.CreatePlayerPreviewUri(_playerPreviewForm.Name, _playerPreviewForm.Position), _cts.Token);

        if (response!.HeadshotUrl is null)
        {
            SnackBar.Add("Player headshot not found - check the name or position.", Severity.Warning);
        }

        _previewHeadshotUrl = response.HeadshotUrl ?? string.Empty;

        _isPreviewButtonDisabled = false;
    }

    private void MapToHeadshotForm()
    {
        _playerPreviewForm.Name = _addPlayerForm.Name;
        _playerPreviewForm.Position = _addPlayerForm.Position;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class PlayerHeadshotService : IPlayerHeadshotService
{
    private readonly HttpClient _httpClient;

    public PlayerHeadshotService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> FindPlayerHeadshotUrlAsync(string fullName, string position, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<PlayerPreviewResponse>(AddPlayerRouteFactory.CreatePlayerPreviewUri(fullName, position), cancellationToken);

        return response!.HeadshotUrl;
    }
}