using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.FreeAgents;

[Authorize]
public partial class Detail : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [Inject]
    private IBidAmountValidator BidAmountValidator { get; set; } = null!;
    [Parameter] public int PlayerId { get; set; }

    private FreeAgentDetailResult? _result;
    private AddBidRequest _form = new();
    private bool _processingForm;
    private const string _title = "Free Agent Details";
    private AddBidRequestValidator _validator = null!;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _validator = new(BidAmountValidator);

        await RefreshPageStateAsync();
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("freeagents/addbid", _form);

            if (response.IsSuccessStatusCode)
            {
                await RefreshPageStateAsync();
                SnackBar.Add("Successfully added bid.", Severity.Success);
            }
            else
            {
                SnackBar.Add("Something went wrong...", Severity.Error);
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        _processingForm = false;
    }

    private async Task RefreshPageStateAsync()
    {
        _form = new() { PlayerId = PlayerId };

        try
        {
            _result = await HttpClient.GetFromJsonAsync<FreeAgentDetailResult>($"freeagents/{PlayerId}", _cts.Token);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class BidAmountValidator : IBidAmountValidator
{
    private readonly HttpClient _httpClient;

    public BidAmountValidator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsHighestBidAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var uri = QueryHelpers.AddQueryString("freeagents/addbid", new Dictionary<string, string>
        {
            { nameof(AddBidRequest.PlayerId), request.PlayerId.ToString() },
            { nameof(AddBidRequest.Amount), request.Amount.ToString() }
        });

        return await _httpClient.GetFromJsonAsync<bool>(uri, cancellationToken);
    }
}
