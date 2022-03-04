using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public partial class SignPlayer : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int PlayerId { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSignPlayerButtonClick { get; set; }
    private SignPlayerRequest _form = null!;
    private SignPlayerDetailResult? _signPlayerDetailResult;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _form = new SignPlayerRequest { PlayerId = PlayerId };

        await SignPlayerDetailsAsnyc();
    }

    private async Task SignPlayerDetailsAsnyc()
    {
        try
        {
            var queryString = QueryHelpers.AddQueryString("api/teams/signplayer", nameof(SignPlayerRequest.PlayerId), PlayerId.ToString());
            _signPlayerDetailResult = await HttpClient.GetFromJsonAsync<SignPlayerDetailResult>(queryString, _cts.Token) ?? new();
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
            var response = await HttpClient.PostAsJsonAsync("api/teams/signplayer", _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Successfully signed player.", Severity.Success);
                await OnSignPlayerButtonClick.InvokeAsync();
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
        MudDialogInstance.Close();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
