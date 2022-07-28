using DynamoLeagueBlazor.Shared.Features.FreeAgents;

namespace DynamoLeagueBlazor.Client.Features.FreeAgents;

public sealed partial class Detail : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [Inject] private IBidValidator BidValidator { get; set; } = null!;
    [Parameter] public int PlayerId { get; set; }

    private FreeAgentDetailResult? _result;
    private AddBidRequest _form = new();
    private bool _processingForm;
    private const string _title = "Free Agent Details";
    private AddBidRequestValidator _validator = null!;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _validator = new(BidValidator);

        await RefreshPageStateAsync();
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(AddBidRouteFactory.Uri, _form);

        if (response.IsSuccessStatusCode)
        {
            await RefreshPageStateAsync();
            SnackBar.Add("Successfully added bid.", Severity.Success);
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }

        _processingForm = false;
    }

    private async Task RefreshPageStateAsync()
    {
        _form = new() { PlayerId = PlayerId };

        _result = await HttpClient.GetFromJsonAsync<FreeAgentDetailResult>(FreeAgentDetailFactory.Create(PlayerId), _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class BidValidator : IBidValidator
{
    private readonly HttpClient _httpClient;

    public BidValidator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> HasNotEndedAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var uri = AddBidRouteFactory.Create(AddBidRouteFactory.GetHasNotEndedUri, request);
        return await _httpClient.GetFromJsonAsync<bool>(uri, cancellationToken);
    }

    public async Task<bool> IsHighestAsync(AddBidRequest request, CancellationToken cancellationToken)
    {
        var uri = AddBidRouteFactory.Create(AddBidRouteFactory.GetIsHighestUri, request);
        return await _httpClient.GetFromJsonAsync<bool>(uri, cancellationToken);
    }
}