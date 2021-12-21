using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.FreeAgents;

[Authorize]
public partial class Detail : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [Parameter] public int PlayerId { get; set; }

    private FreeAgentDetailResult? _result;
    private AddBidRequest _form = new();
    private bool _processingForm;
    private const string _title = "Free Agent Details";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<FreeAgentDetailResult>($"freeagents/{PlayerId}", _cts.Token);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("freeagents/addbid", _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Successfully added fine.", Severity.Success);
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

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
