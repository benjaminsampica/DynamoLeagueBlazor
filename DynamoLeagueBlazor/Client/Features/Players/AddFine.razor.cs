using DynamoLeagueBlazor.Shared.Features.Fines;
using DynamoLeagueBlazor.Shared.Features.Players;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Net.Http.Json;
using System.Text.Json;

namespace DynamoLeagueBlazor.Client.Features.Players;

public partial class AddFine : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int PlayerId { get; set; }

    private AddFineRequest _form = null!;
    private FineDetailResult _result = new() { FineAmount = "Unknown", ContractValue = "Unknown" };
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _form = new AddFineRequest { PlayerId = PlayerId };

        await GetPlayerFineDetailsAsync();
    }

    private async Task GetPlayerFineDetailsAsync()
    {
        try
        {
            var queryString = QueryHelpers.AddQueryString("players/finedetail", nameof(FineDetailRequest.PlayerId), PlayerId.ToString());
            var response = await HttpClient.GetAsync(queryString, _cts.Token);

            if (response.IsSuccessStatusCode)
            {
                _result = await JsonSerializer.DeserializeAsync<FineDetailResult>(await response.Content.ReadAsStreamAsync()) ?? new() { FineAmount = "Unknown", ContractValue = "Unknown" };
            }
            else
            {
                var content = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync());

                foreach (var propertyError in content.Errors)
                {
                    var message = string.Join(", ", propertyError.Value);

                    SnackBar.Add(message, Severity.Error);
                }
            }
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
            var response = await HttpClient.PostAsJsonAsync("players/addfine", _form, _cts.Token);

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
        MudDialogInstance.Close();
    }

    private void Cancel() => MudDialogInstance.Close();

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
