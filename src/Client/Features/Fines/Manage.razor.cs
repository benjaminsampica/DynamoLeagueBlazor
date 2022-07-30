using DynamoLeagueBlazor.Shared.Features.Fines;

namespace DynamoLeagueBlazor.Client.Features.Fines;

public partial class Manage
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int FineId { get; set; }

    private ManageFineRequest _form = null!;
    private bool _processingForm;

    protected override void OnInitialized()
    {
        _form = new ManageFineRequest { FineId = FineId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(ManageFineRouteFactory.Uri, _form);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully updated fine.", Severity.Success);
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }

        _processingForm = false;
        MudDialogInstance.Close();
    }

    private void IsApproved(bool approved)
    {
        _form.Approved = approved;
    }
}
